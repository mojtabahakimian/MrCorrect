using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Prg_Proccessy.AUDIT
{
    /// <summary>
    /// موتور ثبت سابقه.
    ///
    /// قرارداد اصلی: فراخوانی از سمت رابط کاربری فقط یک نوشتن در صف حافظه
    /// است — بدون قفل، بدون I/O، بدون رفت‌وبرگشت به دیتابیس. تمام کار
    /// واقعی روی یک نخ پس‌زمینه‌ی واحد انجام می‌شود و رویدادها دسته‌ای
    /// نوشته می‌شوند.
    ///
    /// چهار قید طراحی:
    ///   • سرعت — هزینه‌ی سمت فراخوان چند انتساب و یک TryWrite است.
    ///   • حافظه — صف کران‌دار است؛ اگر دیتابیس قطع شود حافظه رشد نمی‌کند.
    ///   • همزمانی — Channel برای تولیدکننده‌ها thread-safe و بدون قفل است و
    ///     تنها یک مصرف‌کننده دارد، پس هیچ رقابتی روی خود صف نیست.
    ///   • عدم تداخل — نوشتن سابقه هرگز در تراکنش کاربر شرکت نمی‌کند و هیچ
    ///     خطایی از اینجا به کد فراخوان برنمی‌گردد.
    /// </summary>
    public static class AuditService
    {
        // ── تنظیمات ──────────────────────────────────────────────────────
        private const int QueueCapacity = 20_000;
        private const int BatchMaxRows = 400;
        private const int InsertChunkRows = 100;   // ۱۰۰ ردیف × ~۲۰ پارامتر، زیر سقف ۲۱۰۰ پارامتری SQL Server
        private const int LingerMs = 750;
        private const int FlushRetries = 3;
        private const int MaxSpillFiles = 200;

        // ── حالت ─────────────────────────────────────────────────────────
        private static readonly object _startLock = new();
        private static Channel<AuditEvent>? _channel;
        private static Task? _worker;
        private static CancellationTokenSource? _cts;
        private static string? _connectionString;
        private static AuditSessionInfo? _session;
        private static volatile bool _running;
        private static volatile bool _schemaReady;
        private static int _seq;
        private static int _enqueued;
        private static int _dropped;
        private static int _written;

        /// <summary>نشست جاری. تا وقتی <see cref="Start"/> صدا زده نشده null است.</summary>
        public static AuditSessionInfo? CurrentSession => _session;

        /// <summary>آیا موتور در حال کار است.</summary>
        /// <summary>آیا شنونده‌ی مرکزی دستورهای SQL وصل است.</summary>
        public static bool ListenerAttached => AuditCommandListener.IsAttached;

        public static bool IsRunning => _running;

        /// <summary>تعداد رویدادهایی که به‌خاطر پر بودن صف از دست رفته‌اند. صفر نبودنش یعنی مشکلی هست.</summary>
        public static int DroppedCount => Volatile.Read(ref _dropped);

        /// <summary>تعداد رویدادهای نوشته‌شده روی دیتابیس.</summary>
        public static int WrittenCount => Volatile.Read(ref _written);

        /// <summary>
        /// آیا ساختار جدول‌های سابقه آماده است.
        ///
        /// اگر کاربرِ SQL دسترسی CREATE TABLE نداشته باشد، ساخت ساختار شکست
        /// می‌خورد و سیستم سابقه بی‌صدا غیرفعال می‌ماند — چون قرار نیست کار
        /// کاربر متوقف شود. این پرچم به فرم گزارش اجازه می‌دهد همین وضعیت را
        /// به مدیر نشان دهد، به‌جای اینکه «رکوردی یافت نشد» گمراه‌کننده باشد.
        /// </summary>
        public static bool SchemaReady => _schemaReady;

        /// <summary>شماره‌ی ترتیبی بعدی در این نشست.</summary>
        internal static int NextSeq() => Interlocked.Increment(ref _seq);

        // ── راه‌اندازی ────────────────────────────────────────────────────

        /// <summary>
        /// راه‌اندازی موتور. باید بعد از مشخص شدن کاربر (پس از لاگین) یک بار
        /// صدا زده شود. فراخوانی دوباره بی‌اثر است.
        ///
        /// هیچ کار کند یا شبکه‌ای روی نخ فراخوان انجام نمی‌شود: ساخت ساختار
        /// دیتابیس و درج ردیف نشست هر دو روی نخ پس‌زمینه رخ می‌دهند.
        /// </summary>
        public static void Start(
            string connectionString,
            int? userId,
            string? userName,
            string? appVersion,
            short? fiscalYear,
            string? databaseName)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return;

            lock (_startLock)
            {
                if (_running) return;

                _connectionString = connectionString;

                _session = new AuditSessionInfo
                {
                    SessionId = Guid.NewGuid(),
                    UserId = userId,
                    UserName = Trim(userName, 50),
                    WindowsUser = Trim(SafeGet(() => Environment.UserName), 64),
                    MachineName = Trim(SafeGet(() => Environment.MachineName), 64),
                    ClientIp = Trim(GetLocalIpAddresses(), 128),
                    AppVersion = Trim(appVersion, 40),
                    OsVersion = Trim(SafeGet(() => Environment.OSVersion.VersionString), 100),
                    ProcessId = SafeGetInt(() => Environment.ProcessId),
                    FiscalYear = fiscalYear,
                    DatabaseName = Trim(databaseName, 128),
                    StartedAt = DateTime.Now,
                };

                _channel = Channel.CreateBounded<AuditEvent>(new BoundedChannelOptions(QueueCapacity)
                {
                    // Wait باعث می‌شود TryWrite در حالت پر بودن صف بدون بلاک
                    // شدن false برگرداند. حالت‌های Drop* مقدار true برمی‌گردانند
                    // و افتادن رویداد قابل تشخیص نمی‌ماند.
                    FullMode = BoundedChannelFullMode.Wait,
                    // false و نه true: هنگام خروج، اگر نخ پس‌زمینه داخل یک
                    // فراخوانی کندِ SQL گیر کرده باشد، مسیر خاموش‌سازی باید
                    // بتواند باقی‌مانده‌ی صف را بخواند و روی دیسک بگذارد.
                    // با SingleReader = true آن خواندن رفتار تعریف‌نشده بود و
                    // ناچار صرف‌نظر می‌شد — یعنی همان رویدادها با بسته شدن
                    // برنامه از بین می‌رفتند. هزینه‌ی این تغییر ناچیز است.
                    SingleReader = false,
                    SingleWriter = false,
                    AllowSynchronousContinuations = false,
                });

                _cts = new CancellationTokenSource();

                // توکن در یک متغیر محلی گرفته می‌شود، نه از روی فیلد داخل
                // لامبدا: اگر بستن برنامه فیلد را پاک کند، لامبدا نباید روی
                // مرجع null بیفتد.
                var token = _cts.Token;

                _running = true;
                _worker = Task.Run(() => WorkerLoopAsync(token));

                // شنونده‌ی مرکزی دستورهای SQL. باید بعد از راه‌افتادن صف وصل
                // شود، وگرنه رویدادهای اولیه جایی برای نشستن ندارند.
                AuditCommandListener.Attach();
            }
        }

        /// <summary>
        /// اتصال هویت کاربر به نشستی که پیش از لاگین شروع شده است.
        ///
        /// موتور عمداً قبل از لاگین راه می‌افتد تا «ورود ناموفق» هم ثبت شود؛
        /// این متد بعد از ورود موفق، نام و کد کاربر را به همان نشست می‌چسباند.
        /// جایگزینی مرجع <c>_session</c> اتمیک است، پس نخ‌های تولیدکننده
        /// همیشه یا نشست قبلی یا نشست کامل را می‌بینند و هرگز حالت نیمه‌کاره
        /// نمی‌بینند.
        /// </summary>
        public static void AttachUser(int? userId, string? userName, short? fiscalYear = null, string? appVersion = null)
        {
            var current = _session;
            if (current is null) return;

            // خروج و ورود دوباره بدون بسته شدن برنامه ممکن است
            // (WinBase پنجره‌ی لاگین را دوباره باز می‌کند و AuditService
            // همچنان در حال کار است). در آن حالت Start زودتر برمی‌گردد و
            // نشست قبلی باقی می‌ماند؛ اگر فقط نام کاربر عوض شود، یک سطر
            // نشست حاوی رویدادهای دو کاربر مختلف می‌شود و سطح نشست به
            // کاربر دوم نسبت پیدا می‌کند. پس با تغییر واقعی کاربر، نشست
            // تازه‌ای باز می‌شود.
            var switching = current.UserId is not null and not 0
                            && userId is not null and not 0
                            && current.UserId != userId;

            _session = new AuditSessionInfo
            {
                SessionId = switching ? Guid.NewGuid() : current.SessionId,
                UserId = userId ?? current.UserId,
                UserName = Trim(userName, 50) ?? current.UserName,
                WindowsUser = current.WindowsUser,
                MachineName = current.MachineName,
                ClientIp = current.ClientIp,
                AppVersion = Trim(appVersion, 40) ?? current.AppVersion,
                OsVersion = current.OsVersion,
                ProcessId = current.ProcessId,
                FiscalYear = fiscalYear ?? current.FiscalYear,
                DatabaseName = current.DatabaseName,
                // نشست تازه از همین لحظه شروع می‌شود؛ کپی کردن زمان شروعِ
                // نشست قبلی، مدت حضور کاربر دوم را از ابتدای کار کاربر اول
                // نشان می‌داد.
                StartedAt = switching ? DateTime.Now : current.StartedAt,
            };

            // نشست تازه باید سطر خودش را داشته باشد؛ در غیر این صورت فقط
            // سطر موجود به‌روز می‌شود.
            if (switching)
            {
                // نشست قبلی باید بسته شود، وگرنه ENDED_AT آن برای همیشه خالی
                // می‌ماند و در گزارش، نشستِ کاربر قبلی هنوز «باز» به نظر می‌رسد.
                _ = Task.Run(async () =>
                {
                    await CloseSessionRowAsync(current).ConfigureAwait(false);
                    await WriteSessionRowAsync().ConfigureAwait(false);
                });
            }
            else _ = Task.Run(UpdateSessionUserAsync);
        }

        private static async Task UpdateSessionUserAsync()
        {
            var s = _session;
            if (s is null) return;

            // سطر نشست را نخ پس‌زمینه درج می‌کند و ورود کاربر ممکن است زودتر
            // از آن اتفاق بیفتد — مخصوصاً روی دیتابیس تازه که EnsureCreatedAsync
            // اول باید جدول‌ها را بسازد. آن‌وقت این UPDATE روی صفر سطر می‌نشیند
            // و بی‌صدا رد می‌شود، یعنی کل آن اجرا با USER_NAME خالی ثبت می‌شد.
            // پس تا وقتی سطر پیدا شود دوباره تلاش می‌شود.
            for (var attempt = 0; attempt < 12; attempt++)
            {
                try
                {
                    using var db = new SqlConnection(_connectionString);
                    await db.OpenAsync().ConfigureAwait(false);
                    var affected = await db.ExecuteAsync(
                        @"UPDATE [dbo].[SYS_AUDIT_SESSION]
                             SET [USER_ID] = @UserId,
                                 [USER_NAME] = @UserName,
                                 [FISCAL_YEAR] = @FiscalYear,
                                 [APP_VERSION] = @AppVersion
                           WHERE [SESSION_ID] = @SessionId",
                        s, commandTimeout: 30).ConfigureAwait(false);

                    if (affected > 0) return;
                }
                catch (Exception)
                {
                    // دیتابیس هنوز بالا نیامده یا ساختار ساخته نشده.
                }

                // ۲۵۰ms، ۵۰۰ms، ۱s، سپس ۲s تا سقف — روی هم حدود ۲۰ ثانیه.
                var delay = attempt < 3 ? 250 << attempt : 2000;
                await Task.Delay(delay).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// تخلیه‌ی صف و بستن. با تایم‌اوت کوتاه صدا زده می‌شود تا بستن برنامه
        /// را نگه ندارد.
        /// </summary>
        public static async Task ShutdownAsync(int timeoutMs = 3000)
        {
            Channel<AuditEvent>? channel;
            Task? worker;

            lock (_startLock)
            {
                if (!_running) return;
                _running = false;
                channel = _channel;
                worker = _worker;
            }

            // اول شنونده جدا شود تا در حین تخلیه رویداد تازه‌ای نیاید.
            try { AuditCommandListener.Detach(); } catch { }

            try { channel?.Writer.TryComplete(); } catch { }

            if (worker != null)
            {
                try { await Task.WhenAny(worker, Task.Delay(timeoutMs)).ConfigureAwait(false); }
                catch { }
            }

            // فقط Cancel؛ عمداً Dispose نمی‌شود. اگر تخلیه به تایم‌اوت خورده
            // باشد نخ پس‌زمینه هنوز با همین توکن کار می‌کند و Dispose کردنش
            // باعث ObjectDisposedException می‌شود. یک CancellationTokenSource
            // در لحظه‌ی بسته شدن برنامه ارزش این ریسک را ندارد.
            try { _cts?.Cancel(); } catch { }

            // کمی فرصت به نخ پس‌زمینه، شاید خودش تمام شود و چیزی نماند.
            if (worker != null && !worker.IsCompleted)
            {
                try { await Task.WhenAny(worker, Task.Delay(500)).ConfigureAwait(false); } catch { }
            }

            // هرچه در صف مانده روی دیسک می‌رود تا در اجرای بعدی منتقل شود.
            //
            // این کار بی‌قید و شرط انجام می‌شود، حتی اگر نخ پس‌زمینه هنوز زنده
            // باشد: حالت واقعی و خطرناک همین است — دیتابیس کند یا معلق، نخ
            // داخل یک فراخوانی SQL گیر کرده، کاربر برنامه را می‌بندد. اگر
            // اینجا صرف‌نظر کنیم، آن رویدادها هرگز روی دیسک نمی‌روند و با
            // بسته شدن process از بین می‌روند — دقیقاً همان چیزی که «حذف و
            // امضا گم نمی‌شوند» قرار بود جلویش را بگیرد.
            //
            // خواندن همزمان امن است چون Channel با SingleReader = false ساخته
            // شده. بدترین حالت این است که بخشی از رویدادها را نخ پس‌زمینه
            // بردارد و بخشی را این مسیر؛ هیچ‌کدام گم نمی‌شوند.
            try
            {
                if (channel != null)
                {
                    var leftovers = new List<AuditEvent>();
                    while (leftovers.Count < QueueCapacity && channel.Reader.TryRead(out var e))
                    {
                        leftovers.Add(e);
                    }
                    if (leftovers.Count > 0) TrySpill(leftovers);
                }
            }
            catch (Exception) { }

            // دسته‌ای که نخ پس‌زمینه از صف برداشته ولی هنوز ننوشته است.
            // این‌ها در تخلیه‌ی بالا نمی‌آیند چون دیگر در صف نیستند.
            //
            // ریسک پذیرفته‌شده: اگر نخ پس‌زمینه بعد از این لحظه موفق شود
            // بنویسد، همان رویدادها یک بار هم از روی دیسک منتقل می‌شوند و
            // تکراری می‌مانند. در لحظه‌ی بسته شدن برنامه این پنجره خیلی باریک
            // است، و رویداد تکراری از رویداد گم‌شده به‌مراتب بی‌ضررتر است.
            try
            {
                var inFlight = _inFlight;
                if (worker is not null && !worker.IsCompleted && inFlight is { Count: > 0 })
                {
                    TrySpill(inFlight);
                }
            }
            catch (Exception) { }
        }

        // ── تولید ────────────────────────────────────────────────────────

        /// <summary>
        /// افزودن رویداد به صف. این تنها متدی است که از نخ رابط کاربری صدا
        /// زده می‌شود و عمداً هیچ کاری جز یک نوشتن در صف انجام نمی‌دهد.
        /// </summary>
        /// <summary>شمردن رویدادهایی که جای دیگری از دست رفته‌اند، تا شمارنده واقعی بماند.</summary>
        /// <summary>
        /// دسته‌ای که همین حالا در حال نوشتن روی دیتابیس است.
        ///
        /// این رویدادها دیگر در صف نیستند (از آن برداشته شده‌اند) ولی هنوز
        /// روی دیتابیس هم ننشسته‌اند. اگر نخ پس‌زمینه داخل یک فراخوانی SQLِ
        /// معلق گیر کند و کاربر برنامه را ببندد، تخلیه‌ی صف به آن‌ها نمی‌رسد
        /// و با بسته شدن process از بین می‌روند. پس مسیر خاموش‌سازی این را هم
        /// روی دیسک می‌گذارد.
        /// </summary>
        private static volatile List<AuditEvent>? _inFlight;

        internal static void CountDropped(int count)
        {
            if (count > 0) Interlocked.Add(ref _dropped, count);
        }

        internal static void Enqueue(AuditEvent evt)
        {
            var channel = _channel;

            // موتور هنوز راه نیفتاده یا در حال بسته شدن است. رویداد عادی
            // معنایی ندارد، ولی رویداد حساس — حذف، امضا، ورود ناموفق — باید
            // بماند: روی دیسک محلی می‌نشیند و در اجرای بعدی منتقل می‌شود.
            // بدون این، نوشتنی که پیش از لاگین یا حین خروج رخ دهد بی‌رد گم می‌شد.
            if (channel is null || !_running)
            {
                // AppendCriticalSpill و نه TrySpill: دومی برای هر رویداد یک
                // فایل جدا می‌سازد و ۲۰۰ رویداد در این حالت سقف فایل‌ها را
                // پر می‌کرد و بقیه بی‌صدا دور ریخته می‌شدند.
                if (evt.IsCritical) AppendCriticalSpill(evt);
                return;
            }

            if (channel.Writer.TryWrite(evt))
            {
                Interlocked.Increment(ref _enqueued);
                return;
            }

            // صف پر است. رویدادهای عادی دور ریخته می‌شوند (شمرده می‌شوند تا
            // معلوم باشد)، ولی رویداد حساس هرگز از بین نمی‌رود: روی دیسک
            // محلی می‌نشیند و در چرخه‌ی بعدی به دیتابیس منتقل می‌شود.
            // رویداد حساس روی دیسک می‌نشیند و در اجرای بعدی منتقل می‌شود،
            // پس «از دست رفته» نیست. شمردنش باعث می‌شد فرم سوابق به بازرس
            // هشدار بدهد که رویدادی گم شده، در حالی که ثبت شده بود.
            if (evt.IsCritical)
            {
                AppendCriticalSpill(evt);
            }
            else
            {
                Interlocked.Increment(ref _dropped);
            }
        }

        private static readonly object _criticalSpillLock = new();
        private static bool _spillDirReady;

        /// <summary>
        /// نگه‌داشتن یک رویداد حساس روی دیسک وقتی صف پر است.
        ///
        /// عمداً به یک فایل واحد append می‌شود و نه یک فایل تازه به‌ازای هر
        /// رویداد: اگر دیتابیس قطع باشد و کاربر حذف گروهی انجام دهد، ساختن
        /// فایل جدید و شمردن فایل‌های موجود برای هر ردیف، همان کندی‌ای را
        /// می‌سازد که کل این طراحی برای پرهیز از آن است. append یک syscall است.
        /// </summary>
        private static void AppendCriticalSpill(AuditEvent evt)
        {
            try
            {
                var dir = SpillDirectory;

                lock (_criticalSpillLock)
                {
                    // Directory.CreateDirectory اگر پوشه باشد بی‌هزینه برمی‌گردد.
                    // قبلاً نتیجه یک‌بار برای همیشه نگه داشته می‌شد؛ اگر پوشه
                    // بعداً پاک می‌شد (پاک‌سازی دیسک، پروفایل موقت) هر نوشتن
                    // استثنا می‌داد و بی‌صدا بلعیده می‌شد — یعنی همان تضمین
                    // «رویداد حساس گم نمی‌شود» از بین می‌رفت.
                    if (!_spillDirReady || !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        _spillDirReady = true;
                    }

                    var file = Path.Combine(dir, "critical.jsonl");

                    // سقف اندازه تا در قطعی طولانی دیسک پر نشود.
                    try
                    {
                        var info = new FileInfo(file);
                        if (info.Exists && info.Length > 8 * 1024 * 1024) return;
                    }
                    catch (Exception) { }

                    File.AppendAllText(file, JsonSerializer.Serialize(evt) + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception)
            {
            }
        }

        // ── مصرف ─────────────────────────────────────────────────────────

        private static async Task WorkerLoopAsync(CancellationToken ct)
        {
            var reader = _channel?.Reader;
            if (reader is null) return;

            _schemaReady = await AuditSchema.EnsureCreatedAsync(_connectionString!).ConfigureAwait(false);
            if (_schemaReady)
            {
                await WriteSessionRowAsync().ConfigureAwait(false);

                // رویدادهای به‌جا مانده از اجرای قبلی، همین حالا منتقل شوند.
                //
                // انتقال فقط در انتهای FlushAsync انجام می‌شد، یعنی تنها وقتی
                // رویداد تازه‌ای برای نوشتن وجود داشت. ولی فایل روی دیسک دقیقاً
                // وقتی ساخته می‌شود که دیتابیس قطع بوده؛ اگر در اجرای بعدی
                // کاربر کار قابل‌ثبتی نکند، حلقه‌ی مصرف پشت WaitToReadAsync
                // منتظر می‌ماند، FlushAsync هرگز صدا زده نمی‌شود و آن رویدادها
                // — که همه حساس‌اند: حذف، امضا، ورود ناموفق — روی دیسک می‌مانند.
                await DrainSpillAsync().ConfigureAwait(false);
            }

            var batch = new List<AuditEvent>(BatchMaxRows);

            try
            {
                while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
                {
                    batch.Clear();
                    while (batch.Count < BatchMaxRows && reader.TryRead(out var first))
                    {
                        batch.Add(first);
                    }

                    // کمی صبر تا رویدادهای پشت سر هم در یک دسته جمع شوند.
                    if (batch.Count < BatchMaxRows)
                    {
                        try { await Task.Delay(LingerMs, ct).ConfigureAwait(false); }
                        catch (OperationCanceledException) { }

                        while (batch.Count < BatchMaxRows && reader.TryRead(out var more))
                        {
                            batch.Add(more);
                        }
                    }

                    if (batch.Count > 0)
                    {
                        await FlushAsync(batch).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }

            // تخلیه‌ی نهایی هنگام بسته شدن برنامه.
            //
            // تا وقتی صف خالی نشده ادامه می‌دهد. نسخه‌ی قبلی فقط یک دسته
            // می‌خواند و هر چه بیشتر بود بی‌صدا دور ریخته می‌شد — حتی
            // رویدادهای حساس مثل حذف و امضا، بدون هیچ ردی.
            try
            {
                while (true)
                {
                    batch.Clear();
                    while (batch.Count < BatchMaxRows && reader.TryRead(out var last))
                    {
                        batch.Add(last);
                    }
                    if (batch.Count == 0) break;

                    await FlushAsync(batch).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                // اگر تخلیه نیمه‌کاره ماند، باقی‌مانده روی دیسک محلی می‌رود تا
                // در اجرای بعدی منتقل شود.
                try
                {
                    var leftovers = new List<AuditEvent>();
                    while (reader.TryRead(out var rest)) leftovers.Add(rest);
                    if (leftovers.Count > 0) TrySpill(leftovers);
                }
                catch (Exception) { }
            }

            try { await CloseSessionRowAsync().ConfigureAwait(false); }
            catch (Exception) { }
        }

        /// <summary>زمان آخرین تلاش برای ساخت ساختار، برای فاصله‌گذاری بین تلاش‌ها.</summary>
        private static long _lastSchemaAttemptTicks;
        private const int SchemaRetryIntervalMs = 5 * 60 * 1000;

        private static async Task FlushAsync(List<AuditEvent> batch)
        {
            _inFlight = batch;

            if (!_schemaReady)
            {
                // تلاش دوباره برای ساخت ساختار فقط هر چند دقیقه یک بار.
                //
                // اگر کاربرِ SQL دسترسی CREATE TABLE نداشته باشد، ساخت ساختار
                // هر بار شکست می‌خورد. بدون این فاصله‌گذاری، هر چرخه‌ی تخلیه
                // (کمتر از یک ثانیه) دوازده دستور DDL را دوباره می‌فرستاد و
                // دوازده استثنا تولید می‌کرد — تا ابد. یعنی یک تنظیم اشتباهِ
                // دسترسی، به کوبیدن مداوم دیتابیس تبدیل می‌شد.
                var now = Environment.TickCount64;
                var last = Interlocked.Read(ref _lastSchemaAttemptTicks);

                if (last != 0 && now - last < SchemaRetryIntervalMs)
                {
                    // این دسته همین حالا روی دیسک نشست، پس دیگر «در حال
                    // پرواز» نیست. اگر پاک نشود، _inFlight برای همیشه به
                    // همان List اشاره می‌ماند که حلقه‌ی مصرف بارها Clear و
                    // دوباره پر می‌کند؛ آن‌وقت مسیر خاموش‌سازی محتوای فعلیِ
                    // آن — رویدادهایی که قبلاً با موفقیت نوشته شده‌اند — را
                    // دوباره روی دیسک می‌گذارد و در اجرای بعدی ردیف تکراری
                    // درج می‌شود، ضمن اینکه خواندن همزمانِ یک List در حال
                    // تغییر هم هست.
                    _inFlight = null;
                    TrySpill(batch);
                    return;
                }

                Interlocked.Exchange(ref _lastSchemaAttemptTicks, now);

                var wasReady = _schemaReady;
                _schemaReady = await AuditSchema.EnsureCreatedAsync(_connectionString!).ConfigureAwait(false);

                // ساختار تازه آماده شده: سطر نشست هنوز نوشته نشده، چون تلاش
                // اول هنگام راه‌اندازی شکست خورده بود. بدون این، کل رویدادهای
                // این اجرا به نشستی اشاره می‌کنند که وجود ندارد و در فرم
                // سوابق نام کامپیوتر و IP خالی می‌ماند.
                if (_schemaReady && !wasReady)
                {
                    await WriteSessionRowAsync().ConfigureAwait(false);
                }

                if (!_schemaReady)
                {
                    // به همان دلیل بالا: روی دیسک رفت، پس در حال پرواز نیست.
                    _inFlight = null;
                    TrySpill(batch);
                    return;
                }
            }

            // تلاش مجدد در سطح هر تکه انجام می‌شود، نه کل دسته.
            //
            // اگر کل دسته دوباره تلاش شود، تکه‌هایی که قبلاً با موفقیت درج
            // شده‌اند دوباره درج می‌شوند و چون جدول کلید طبیعی ندارد، ردیف
            // تکراری برای همیشه می‌ماند. با این ساختار، هر تکه یا یک بار
            // نوشته می‌شود یا اصلاً نوشته نمی‌شود.
            var index = 0;
            while (index < batch.Count)
            {
                var chunk = batch.GetRange(index, Math.Min(InsertChunkRows, batch.Count - index));
                var ok = false;

                for (var attempt = 0; attempt <= FlushRetries && !ok; attempt++)
                {
                    try
                    {
                        using var db = new SqlConnection(_connectionString);
                        await db.OpenAsync().ConfigureAwait(false);

                        await InsertEventsAsync(db, chunk).ConfigureAwait(false);
                        ok = true;

                        // نوشتن در جدول‌های قدیمی عمداً بعد از ok انجام می‌شود:
                        // اگر شکست بخورد نباید باعث تلاش دوباره‌ی همین تکه شود،
                        // چون آن‌وقت ردیف‌های جریان اصلی تکراری درج می‌شدند.
                        await InsertLegacyAsync(db, chunk).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        if (attempt >= FlushRetries) break;
                        try { await Task.Delay(250 * (attempt + 1)).ConfigureAwait(false); } catch { }
                    }
                }

                if (!ok)
                {
                    // دیتابیس در دسترس نیست. این تکه و هر چه بعد از آن مانده
                    // روی دیسک محلی نگه داشته می‌شود؛ تکه‌های نوشته‌شده دوباره
                    // ذخیره نمی‌شوند تا ردیف تکراری ایجاد نشود.
                    TrySpill(batch.GetRange(index, batch.Count - index));
                    _inFlight = null;
                    return;
                }

                Interlocked.Add(ref _written, chunk.Count);
                index += chunk.Count;
            }

            // دسته با موفقیت نوشته شد؛ دیگر «در حال پرواز» نیست.
            _inFlight = null;

            try
            {
                using var db = new SqlConnection(_connectionString);
                await db.OpenAsync().ConfigureAwait(false);
                await ReplayOneSpillFileAsync(db).ConfigureAwait(false);
            }
            catch (Exception) { }
        }

        private static async Task InsertEventsAsync(SqlConnection db, List<AuditEvent> rows)
        {
            var sql = new StringBuilder(rows.Count * 120);
            sql.Append(
                "INSERT INTO [dbo].[SYS_AUDIT_EVENT] " +
                "([SESSION_ID],[SEQ],[USER_ID],[USER_NAME],[AT_CLIENT],[DATE_S],[TIME_S]," +
                "[CATEGORY],[SEVERITY],[ACTION],[ENTITY],[ENTITY_KEY],[FORM_NAME],[TITLE]," +
                "[DETAIL],[IS_SUCCESS],[ERR_MSG],[DURATION_MS],[CORR_ID]) VALUES ");

            var p = new DynamicParameters();

            for (var i = 0; i < rows.Count; i++)
            {
                var e = rows[i];
                if (i > 0) sql.Append(',');
                sql.Append("(@s").Append(i).Append(",@q").Append(i).Append(",@u").Append(i)
                   .Append(",@n").Append(i).Append(",@t").Append(i).Append(",@d").Append(i)
                   .Append(",@m").Append(i).Append(",@c").Append(i).Append(",@v").Append(i)
                   .Append(",@a").Append(i).Append(",@e").Append(i).Append(",@k").Append(i)
                   .Append(",@f").Append(i).Append(",@l").Append(i).Append(",@j").Append(i)
                   .Append(",@o").Append(i).Append(",@r").Append(i).Append(",@w").Append(i)
                   .Append(",@x").Append(i).Append(')');

                p.Add("@s" + i, e.SessionId == Guid.Empty ? (Guid?)null : e.SessionId);
                p.Add("@q" + i, e.Seq);
                p.Add("@u" + i, e.UserId);
                p.Add("@n" + i, Trim(e.UserName, 50));
                p.Add("@t" + i, e.AtClient);
                p.Add("@d" + i, e.DateS);
                p.Add("@m" + i, e.TimeS);
                p.Add("@c" + i, (byte)e.Category);
                p.Add("@v" + i, (byte)e.Severity);
                p.Add("@a" + i, Trim(e.Action, 32));
                p.Add("@e" + i, Trim(e.Entity, 100));
                p.Add("@k" + i, Trim(e.EntityKey, 80));
                p.Add("@f" + i, Trim(e.FormName, 64));
                p.Add("@l" + i, Trim(e.Title, 250));
                p.Add("@j" + i, e.Detail);
                p.Add("@o" + i, e.IsSuccess);
                p.Add("@r" + i, Trim(e.ErrorMessage, 400));
                p.Add("@w" + i, e.DurationMs);
                p.Add("@x" + i, e.CorrelationId);
            }

            await db.ExecuteAsync(sql.ToString(), p, commandTimeout: 60).ConfigureAwait(false);
        }

        /// <summary>
        /// نوشتن در جدول‌های سابقه‌ی قدیمی. این جدول‌ها حذف نشده‌اند و برای
        /// سازگاری همچنان پر می‌شوند — با این تفاوت که حالا از همین نخ
        /// پس‌زمینه نوشته می‌شوند و دیگر روی نخ رابط کاربری اجرا نمی‌شوند.
        /// </summary>
        private static async Task InsertLegacyAsync(SqlConnection db, List<AuditEvent> rows)
        {
            var amaliat = rows.Where(r => r.Legacy == AuditLegacyTarget.Amaliat).ToList();
            if (amaliat.Count > 0)
            {
                try
                {
                    var sql = new StringBuilder("INSERT INTO [dbo].[AMALIAT] ([USERID],[USERNAME],[ADATE],[AMALID]) VALUES ");
                    var p = new DynamicParameters();
                    for (var i = 0; i < amaliat.Count; i++)
                    {
                        var e = amaliat[i];
                        if (i > 0) sql.Append(',');
                        sql.Append("(@au").Append(i).Append(",@an").Append(i)
                           .Append(",@ad").Append(i).Append(",@ai").Append(i).Append(')');

                        // USERCOD در Baseknow وقتی null باشد 0 برمی‌گرداند؛ همان
                        // رفتار قبلی اینجا حفظ می‌شود تا ردیف‌های AMALIAT عوض نشوند.
                        p.Add("@au" + i, e.UserId ?? 0);
                        p.Add("@an" + i, Trim("MCR | " + e.UserName, 49));
                        p.Add("@ad" + i, e.AtClient);
                        p.Add("@ai" + i, Trim(e.FormName, 49));
                    }
                    await db.ExecuteAsync(sql.ToString(), p, commandTimeout: 60).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // جدول قدیمی روی همه‌ی پایگاه‌ها وجود ندارد؛ نبودنش نباید
                    // نوشتن جریان جدید را خراب کند.
                }
            }

            var legacyAudit = rows.Where(r => r.Legacy == AuditLegacyTarget.UserAuditLog).ToList();
            if (legacyAudit.Count > 0)
            {
                try
                {
                    var s = _session;
                    var sql = new StringBuilder(
                        "INSERT INTO [dbo].[USER_AUDIT_LOG] " +
                        "([UserName],[WindowsUserName],[ActionType],[TableName],[RecordID],[OldValue],[NewValue]," +
                        "[IPAddress],[MachineName],[ApplicationVersion],[WindowsVersion],[ActionDateTime]," +
                        "[AdditionalInfo],[SessionID],[ProcessID],[ThreadID],[StackTrace],[IsSuccess],[ErrorMessage]) VALUES ");
                    var p = new DynamicParameters();
                    for (var i = 0; i < legacyAudit.Count; i++)
                    {
                        var e = legacyAudit[i];
                        if (i > 0) sql.Append(',');
                        sql.Append("(@lu").Append(i).Append(",@lw").Append(i).Append(",@la").Append(i)
                           .Append(",@lt").Append(i).Append(",@lr").Append(i).Append(",@lo").Append(i)
                           .Append(",@ln").Append(i).Append(",@li").Append(i).Append(",@lm").Append(i)
                           .Append(",@lv").Append(i).Append(",@lz").Append(i).Append(",@ld").Append(i)
                           .Append(",@lj").Append(i).Append(",@ls").Append(i).Append(",@lp").Append(i)
                           .Append(",@lh").Append(i).Append(",NULL,@lk").Append(i).Append(",@le").Append(i).Append(')');

                        // UserName / ActionType / TableName در جدول قدیمی
                        // NOT NULL هستند؛ اگر رویدادی پیش از لاگین یا بدون
                        // موجودیت ثبت شود، بدون این جایگزینی کل دسته رد می‌شود.
                        p.Add("@lu" + i, Trim(e.UserName, 100) ?? string.Empty);
                        p.Add("@lw" + i, Trim(s?.WindowsUser, 100));
                        p.Add("@la" + i, Trim(e.Action, 50) ?? string.Empty);
                        p.Add("@lt" + i, Trim(e.Entity, 100) ?? string.Empty);
                        p.Add("@lr" + i, Trim(e.EntityKey, 100));
                        p.Add("@lo" + i, e.LegacyOldValue);
                        p.Add("@ln" + i, e.LegacyNewValue);
                        p.Add("@li" + i, Trim(s?.ClientIp, 50));
                        p.Add("@lm" + i, Trim(s?.MachineName, 100));
                        p.Add("@lv" + i, Trim(s?.AppVersion, 50));
                        p.Add("@lz" + i, Trim(s?.OsVersion, 100));
                        p.Add("@ld" + i, e.AtClient);
                        p.Add("@lj" + i, e.Detail);
                        p.Add("@ls" + i, e.SessionId == Guid.Empty ? (Guid?)null : e.SessionId);
                        p.Add("@lp" + i, s?.ProcessId);
                        p.Add("@lh" + i, e.Seq);
                        p.Add("@lk" + i, e.IsSuccess);
                        p.Add("@le" + i, e.ErrorMessage);
                    }
                    await db.ExecuteAsync(sql.ToString(), p, commandTimeout: 60).ConfigureAwait(false);
                }
                catch (Exception)
                {
                }
            }
        }

        private static async Task WriteSessionRowAsync()
        {
            var s = _session;
            if (s is null) return;

            try
            {
                using var db = new SqlConnection(_connectionString);
                await db.OpenAsync().ConfigureAwait(false);
                await db.ExecuteAsync(
                    @"IF NOT EXISTS (SELECT 1 FROM [dbo].[SYS_AUDIT_SESSION] WHERE [SESSION_ID] = @SessionId)
                      INSERT INTO [dbo].[SYS_AUDIT_SESSION]
                          ([SESSION_ID],[USER_ID],[USER_NAME],[WIN_USER],[MACHINE_NAME],[CLIENT_IP],
                           [APP_VERSION],[OS_VERSION],[PROCESS_ID],[FISCAL_YEAR],[DB_NAME],[STARTED_AT])
                      VALUES (@SessionId,@UserId,@UserName,@WindowsUser,@MachineName,@ClientIp,
                              @AppVersion,@OsVersion,@ProcessId,@FiscalYear,@DatabaseName,@StartedAt)",
                    s, commandTimeout: 60).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        private static async Task CloseSessionRowAsync(AuditSessionInfo? target = null)
        {
            var s = _session;
            if (target != null) s = target;
            if (s is null) return;

            try
            {
                using var db = new SqlConnection(_connectionString);
                await db.OpenAsync().ConfigureAwait(false);
                await db.ExecuteAsync(
                    @"UPDATE [dbo].[SYS_AUDIT_SESSION]
                         SET [ENDED_AT] = SYSDATETIME(),
                             [EVENT_COUNT] = @Written,
                             [DROPPED_COUNT] = @Dropped
                       WHERE [SESSION_ID] = @SessionId",
                    new { s.SessionId, Written = WrittenCount, Dropped = DroppedCount },
                    commandTimeout: 30).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }

        // ── نگه‌داری موقت روی دیسک وقتی دیتابیس در دسترس نیست ─────────────

        /// <summary>
        /// محل نگه‌داری موقت رویدادها وقتی دیتابیس در دسترس نیست.
        ///
        /// عمداً LocalApplicationData است نه CommonApplicationData:
        /// ProgramData به‌صورت پیش‌فرض برای همه‌ی کاربران آن ماشین خواندنی
        /// است و این فایل‌ها نام کاربر، IP، نام کامپیوتر و شماره‌ی اسناد را
        /// به‌صورت متن ساده دارند. LocalApplicationData به پروفایل همان
        /// کاربر محدود است.
        /// </summary>
        private static string SpillDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MrCorrect", "AuditSpill");

        private static void TrySpill(IReadOnlyCollection<AuditEvent> rows)
        {
            if (rows.Count == 0) return;

            try
            {
                var dir = SpillDirectory;
                Directory.CreateDirectory(dir);

                // سقف تعداد فایل: اگر دیتابیس مدت طولانی قطع باشد، دیسک نباید پر شود.
                //
                // این یک از دست رفتن واقعی است، پس شمرده می‌شود. قبلاً بی‌صدا
                // برمی‌گشت و شمارنده‌ی «چند رویداد از دست رفت» — همانی که در
                // فرم سوابق هشدار می‌دهد — این‌ها را نمی‌دید، یعنی حتی حذف و
                // امضا می‌توانستند بی‌هیچ ردی گم شوند.
                if (Directory.EnumerateFiles(dir, "*.jsonl").Take(MaxSpillFiles + 1).Count() > MaxSpillFiles)
                {
                    Interlocked.Add(ref _dropped, rows.Count);
                    return;
                }

                var file = Path.Combine(dir, $"audit_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.jsonl");
                var sb = new StringBuilder(rows.Count * 200);
                foreach (var r in rows)
                {
                    sb.AppendLine(JsonSerializer.Serialize(r));
                }
                File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// همه‌ی فایل‌های به‌جا مانده را پشت سر هم منتقل می‌کند. اگر یک دور
        /// هیچ فایلی کم نکند (دیتابیس هنوز قطع است) بیرون می‌آید تا بی‌جهت
        /// نچرخد؛ دور بعدی در انتهای اولین Flush دوباره تلاش می‌شود.
        /// </summary>
        private static async Task DrainSpillAsync()
        {
            try
            {
                var dir = SpillDirectory;
                if (!Directory.Exists(dir)) return;

                static int Count(string d) =>
                    Directory.EnumerateFiles(d, "*.jsonl").Take(MaxSpillFiles + 1).Count();

                if (Count(dir) == 0) return;

                using var db = new SqlConnection(_connectionString);
                await db.OpenAsync().ConfigureAwait(false);

                for (var i = 0; i < MaxSpillFiles; i++)
                {
                    var before = Count(dir);
                    if (before == 0) break;

                    await ReplayOneSpillFileAsync(db).ConfigureAwait(false);

                    // پیشرفتی نشد: یا دیتابیس در دسترس نیست یا فایل قابل
                    // انتقال نیست. ادامه‌ی حلقه فقط وقت تلف می‌کند.
                    if (Count(dir) >= before) break;
                }
            }
            catch (Exception)
            {
            }
        }

        private static async Task ReplayOneSpillFileAsync(SqlConnection db)
        {
            try
            {
                var dir = SpillDirectory;
                if (!Directory.Exists(dir)) return;

                var file = Directory.EnumerateFiles(dir, "*.jsonl").FirstOrDefault();
                if (file is null) return;

                // فایل رویدادهای حساس همچنان در حال append شدن است. پیش از
                // خواندن، زیر همان قفل به یک نام تازه منتقل می‌شود تا
                // نوشتن‌های همزمان روی فایل جدید بروند و چیزی گم نشود.
                if (Path.GetFileName(file).Equals("critical.jsonl", StringComparison.OrdinalIgnoreCase))
                {
                    var rotated = Path.Combine(dir, $"critical_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.jsonl");
                    lock (_criticalSpillLock)
                    {
                        if (!File.Exists(file)) return;
                        File.Move(file, rotated);
                    }
                    file = rotated;
                }

                var rows = new List<AuditEvent>();
                foreach (var line in File.ReadLines(file))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // یک خط ناقص (مثلاً وقتی برنامه وسط نوشتن بسته شده) نباید
                    // کل فایل را زمین‌گیر کند: بدون این try، استثنا از حلقه
                    // بیرون می‌زد، فایل نه پاک می‌شد نه بازنویسی، و چون همیشه
                    // اولین فایل انتخاب می‌شود، پخش تا سقف ۲۰۰ فایل قفل می‌ماند
                    // و از آن به بعد همه چیز دور ریخته می‌شد.
                    try
                    {
                        var e = JsonSerializer.Deserialize<AuditEvent>(line);
                        if (e != null) rows.Add(e);
                    }
                    catch (JsonException)
                    {
                    }
                }

                // اگر درج تکه‌ی دوم شکست بخورد، فایل نباید دست‌نخورده بماند:
                // در چرخه‌ی بعدی از اول خوانده می‌شود و تکه‌ی اول دوباره درج
                // می‌گردد. پس فایل با باقی‌مانده بازنویسی می‌شود.
                var done = 0;
                try
                {
                    for (var i = 0; i < rows.Count; i += InsertChunkRows)
                    {
                        var chunk = rows.GetRange(i, Math.Min(InsertChunkRows, rows.Count - i));
                        await InsertEventsAsync(db, chunk).ConfigureAwait(false);
                        done += chunk.Count;
                    }
                }
                finally
                {
                    if (done >= rows.Count)
                    {
                        File.Delete(file);
                    }
                    else if (done > 0)
                    {
                        var remaining = new StringBuilder();
                        for (var i = done; i < rows.Count; i++)
                        {
                            remaining.AppendLine(JsonSerializer.Serialize(rows[i]));
                        }
                        File.WriteAllText(file, remaining.ToString(), Encoding.UTF8);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // ── کمکی‌ها ──────────────────────────────────────────────────────

        /// <summary>
        /// آدرس‌های IPv4 محلی. عمداً از Dns.GetHostEntry استفاده نمی‌شود:
        /// آن متد یک فراخوانی DNS مسدودکننده است و اگر سرور نام کند باشد،
        /// نخ فراخوان را ثانیه‌ها معطل می‌کند.
        /// </summary>
        private static string GetLocalIpAddresses()
        {
            try
            {
                var list = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                                n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address.ToString())
                    .Distinct()
                    .Take(4)
                    .ToArray();

                return list.Length == 0 ? string.Empty : string.Join(";", list);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        internal static string? Trim(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static string SafeGet(Func<string> get)
        {
            try { return get() ?? string.Empty; }
            catch (Exception) { return string.Empty; }
        }

        private static int SafeGetInt(Func<int> get)
        {
            try { return get(); }
            catch (Exception) { return 0; }
        }
    }
}
