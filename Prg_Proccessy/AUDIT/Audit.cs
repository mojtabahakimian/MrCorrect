using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;

namespace Prg_Proccessy.AUDIT
{
    /// <summary>
    /// نقطه‌ی ورود ثبت سابقه. تمام کد نرم‌افزار فقط با این کلاس کار می‌کند.
    ///
    /// هیچ‌کدام از متدهای اینجا استثنا پرتاب نمی‌کنند و هیچ‌کدام منتظر
    /// دیتابیس نمی‌مانند؛ کار کاربر تحت هیچ شرایطی نباید به‌خاطر سابقه
    /// کند یا متوقف شود.
    /// </summary>
    public static class Audit
    {
        private static readonly PersianCalendar _persian = new();

        /// <summary>
        /// جلوگیری از تکرار رویداد «باز شدن فرم». کاربر ممکن است یک فرم را
        /// در چند ثانیه چند بار باز و بسته کند؛ بدون این کنترل، پرحجم‌ترین
        /// دسته‌ی رویدادها بی‌دلیل چند برابر می‌شود.
        /// اندازه‌ی این دیکشنری ذاتاً محدود است (به تعداد فرم‌های نرم‌افزار).
        /// </summary>
        private static readonly ConcurrentDictionary<string, long> _lastFormOpen = new(StringComparer.OrdinalIgnoreCase);
        private const int FormDedupWindowMs = 15_000;

        /// <summary>شناسه‌ی جدید برای گروه‌کردن رویدادهای یک عملیات چندمرحله‌ای.</summary>
        public static Guid NewCorrelation() => Guid.NewGuid();

        // ── ناوبری ───────────────────────────────────────────────────────

        /// <summary>ثبت باز شدن یک فرم.</summary>
        /// <summary>
        /// آخرین فرمی که کاربر باز کرده.
        ///
        /// رویدادهای DML از شنونده‌ی مرکزی می‌آیند و آنجا معلوم نیست دستور از
        /// کدام پنجره آمده. بدون این، در خط زمانی می‌شد فهمید «HEAD_LST ویرایش
        /// شد» ولی نه از کدام فرم — فاکتور فروش، پیش‌فاکتور یا حواله.
        ///
        /// عمداً یک مقدار سراسری است و نه thread-local: دستور ممکن است روی نخ
        /// پس‌زمینه اجرا شود ولی همچنان نتیجه‌ی کاری باشد که کاربر در همان فرم
        /// شروع کرده. پس این «زمینه» است، نه سند قطعی؛ کلید و جدولِ خودِ
        /// رویداد همچنان مرجع دقیق‌اند.
        /// </summary>
        internal static string? CurrentForm => _currentForm;
        private static volatile string? _currentForm;

        /// <summary>
        /// فقط زمینه‌ی فرم جاری را به‌روز می‌کند، بدون ساختن رویداد.
        ///
        /// وقتی کاربر به پنجره‌ای که از قبل باز است برمی‌گردد، «باز کردن فرم»
        /// دوباره ثبت نمی‌شود — ولی زمینه باید همان پنجره شود، وگرنه
        /// نوشتن‌های بعدی به نام فرمی می‌خورد که آخرین بار باز شده بود.
        /// </summary>
        public static void SetCurrentForm(string? formName)
        {
            if (!string.IsNullOrWhiteSpace(formName)) _currentForm = formName;
        }

        /// <param name="setContext">
        /// آیا این فرم، «فرم جاری» هم بشود. برای بررسی دسترسی (LETSGO) باید
        /// false باشد: آنجا کدهایی مثل «chartfilter» پاس داده می‌شود که پنجره
        /// نیستند، و اگر زمینه را عوض کنند نوشتن‌های بعدی به نام یک کدِ
        /// دسترسی نسبت داده می‌شوند نه به پنجره‌ی واقعی.
        /// </param>
        public static void Form(string? formName, string? persianTitle = null, bool setContext = true)
        {
            if (string.IsNullOrWhiteSpace(formName)) return;

            // پیش از فیلتر تکرار تنظیم می‌شود: اگر کاربر به فرمی برگردد که
            // کمتر از پانزده ثانیه پیش باز کرده، رویداد تازه‌ای ثبت نمی‌شود
            // ولی زمینه باید همان فرم باشد.
            if (setContext) _currentForm = formName;

            try
            {
                var now = Environment.TickCount64;
                if (_lastFormOpen.TryGetValue(formName, out var last) &&
                    now - last < FormDedupWindowMs)
                {
                    return;
                }
                _lastFormOpen[formName] = now;

                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Navigation,
                    Action = AuditAction.OpenForm,
                    FormName = formName,
                    Title = persianTitle ?? ("باز کردن فرم " + formName),
                    Legacy = AuditLegacyTarget.Amaliat,
                });
            }
            catch (Exception) { }
        }

        // ── تغییر داده ───────────────────────────────────────────────────

        public static void Insert(string entity, string? entityKey, string? persianTitle = null,
                                  string? formName = null, object? newValue = null, Guid? correlationId = null)
            => Data(AuditAction.Insert, entity, entityKey, persianTitle, formName, null, newValue, correlationId);

        public static void Update(string entity, string? entityKey, string? persianTitle = null,
                                  string? formName = null, object? oldValue = null, object? newValue = null,
                                  Guid? correlationId = null)
            => Data(AuditAction.Update, entity, entityKey, persianTitle, formName, oldValue, newValue, correlationId);

        /// <summary>
        /// ثبت حذف. حذف همیشه حساس است، پس به‌عنوان رویداد بحرانی ثبت
        /// می‌شود و حتی در صورت پر بودن صف هم از بین نمی‌رود.
        /// </summary>
        public static void Delete(string entity, string? entityKey, string? persianTitle = null,
                                  string? formName = null, object? oldValue = null, Guid? correlationId = null)
            => Data(AuditAction.Delete, entity, entityKey, persianTitle, formName, oldValue, null, correlationId);

        /// <summary>
        /// مثل <see cref="Data"/>، ولی جزئیات از قبل به‌صورت JSON آماده است.
        /// تشخیص خودکار SQL از این استفاده می‌کند تا ستون‌های تغییرکرده و
        /// مقدار جدیدشان را بدون سریال‌سازی دوباره ثبت کند.
        /// </summary>
        public static void DataWithDetail(string action, string entity, string? entityKey,
                                          string? persianTitle, string? formName, string? detail,
                                          Guid? correlationId = null)
        {
            try
            {
                var isDelete = string.Equals(action, AuditAction.Delete, StringComparison.OrdinalIgnoreCase);

                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Data,
                    Severity = isDelete ? AuditSeverity.Sensitive : AuditSeverity.Notable,
                    Action = action,
                    Entity = entity,
                    EntityKey = entityKey,
                    FormName = formName,
                    Title = persianTitle ?? BuildDataTitle(action, entity, entityKey),
                    Detail = detail,
                    CorrelationId = correlationId,
                    IsCritical = isDelete,
                });
            }
            catch (Exception) { }
        }

        public static void Data(string action, string entity, string? entityKey, string? persianTitle,
                                string? formName, object? oldValue, object? newValue, Guid? correlationId = null)
        {
            try
            {
                var isDelete = string.Equals(action, AuditAction.Delete, StringComparison.OrdinalIgnoreCase);

                var oldJson = ToJson(oldValue);
                var newJson = ToJson(newValue);

                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Data,
                    Severity = isDelete ? AuditSeverity.Sensitive : AuditSeverity.Notable,
                    Action = action,
                    Entity = entity,
                    EntityKey = entityKey,
                    FormName = formName,
                    Title = persianTitle ?? BuildDataTitle(action, entity, entityKey),
                    Detail = BuildDiffDetail(oldJson, newJson),
                    CorrelationId = correlationId,
                    IsCritical = isDelete,
                });
            }
            catch (Exception) { }
        }

        // ── عملیات کاری ──────────────────────────────────────────────────

        /// <summary>
        /// ثبت امضا یا برداشتن امضا. تا امروز فقط وضعیت نهایی در ستون‌های
        /// SGN1..SGN4 می‌ماند و معلوم نیست چه کسی و کِی امضا زده یا برداشته.
        /// </summary>
        public static void Sign(string entity, string? entityKey, int slot, bool signed,
                                string? formName = null, string? persianTitle = null)
        {
            try
            {
                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Business,
                    Severity = AuditSeverity.Sensitive,
                    Action = signed ? AuditAction.Sign : AuditAction.Unsign,
                    Entity = entity,
                    EntityKey = entityKey,
                    FormName = formName,
                    Title = persianTitle ??
                            (signed ? $"امضای {DescribeEntity(entity, entityKey)} (امضای {slot})"
                                    : $"برداشتن امضای {DescribeEntity(entity, entityKey)} (امضای {slot})"),
                    Detail = "{\"slot\":" + slot + ",\"signed\":" + (signed ? "true" : "false") + "}",
                    IsCritical = true,
                });
            }
            catch (Exception) { }
        }

        /// <summary>
        /// ثبت وضعیت کامل امضاها.
        ///
        /// در این نرم‌افزار یک کلیک روی هر خانه‌ی امضا، دستوری می‌سازد که
        /// <b>همه‌ی</b> خانه‌ها را با هم می‌نویسد
        /// (<c>SET SGN1=0, SGN2=0, SGN3=1</c>). پس از روی خودِ دستور نمی‌توان
        /// فهمید کدام خانه عوض شده؛ آنچه قابل ثبت و درست است، وضعیت کاملِ آن
        /// لحظه است. تغییر واقعی از مقایسه‌ی دو رویداد پشت‌سرهم روی همان سند
        /// به‌دست می‌آید و در گزارش هم همین‌طور دیده می‌شود.
        /// </summary>
        public static void SignState(string entity, string? entityKey, bool anySigned,
                                     string persianTitle, IReadOnlyDictionary<int, bool> slots,
                                     string? formName = null)
        {
            try
            {
                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Business,
                    Severity = AuditSeverity.Sensitive,
                    Action = anySigned ? AuditAction.Sign : AuditAction.Unsign,
                    Entity = entity,
                    EntityKey = entityKey,
                    FormName = formName,
                    Title = persianTitle,
                    Detail = JsonSerializer.Serialize(slots, JsonOptions),
                    IsCritical = true,
                });
            }
            catch (Exception) { }
        }

        /// <summary>ثبت تبدیل یک سند به سند دیگر، مثل تبدیل پیش‌فاکتور به فاکتور یا حواله.</summary>
        public static void ConvertDoc(string fromEntity, string? fromKey, string toEntity, string? toKey,
                                   string? formName = null, Guid? correlationId = null)
        {
            try
            {
                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Business,
                    Severity = AuditSeverity.Sensitive,
                    Action = AuditAction.Convert,
                    Entity = fromEntity,
                    EntityKey = fromKey,
                    FormName = formName,
                    Title = $"تبدیل {DescribeEntity(fromEntity, fromKey)} به {DescribeEntity(toEntity, toKey)}",
                    Detail = JsonSerializer.Serialize(new { from = fromEntity, fromKey, to = toEntity, toKey }, JsonOptions),
                    CorrelationId = correlationId,
                    IsCritical = true,
                });
            }
            catch (Exception) { }
        }

        /// <summary>ثبت تایید یا نهایی‌سازی، مثل تایید بارگیری.</summary>
        public static void Confirm(string entity, string? entityKey, string persianTitle,
                                   string? formName = null, Guid? correlationId = null)
        {
            try
            {
                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Business,
                    Severity = AuditSeverity.Sensitive,
                    Action = AuditAction.Confirm,
                    Entity = entity,
                    EntityKey = entityKey,
                    FormName = formName,
                    Title = persianTitle,
                    CorrelationId = correlationId,
                    IsCritical = true,
                });
            }
            catch (Exception) { }
        }

        // ── چاپ و خروجی ──────────────────────────────────────────────────

        /// <summary>
        /// ثبت چاپ یا پیش‌نمایش. باید روی خودِ فرمان چاپ صدا زده شود، نه
        /// موقع باز شدن پنجره‌ی گزارش؛ وگرنه معلوم نمی‌شود کاربر واقعاً چاپ
        /// گرفته یا فقط گزارش را دیده است.
        /// </summary>
        public static void Print(string? reportName, string? entity = null, string? entityKey = null,
                                 bool isPreview = false, string? formName = null, int? copies = null)
        {
            try
            {
                var what = string.IsNullOrWhiteSpace(reportName) ? "گزارش" : reportName!;
                var title = isPreview
                    ? $"پیش‌نمایش {what}"
                    : $"چاپ {what}" + (copies is > 1 ? $" ({copies} نسخه)" : string.Empty);

                if (!string.IsNullOrWhiteSpace(entityKey))
                    title += " — " + DescribeEntity(entity, entityKey);

                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Report,
                    Severity = isPreview ? AuditSeverity.Normal : AuditSeverity.Notable,
                    Action = isPreview ? AuditAction.Preview : AuditAction.Print,
                    Entity = entity,
                    EntityKey = entityKey,
                    FormName = formName ?? reportName,
                    Title = title,
                    IsCritical = !isPreview,
                });
            }
            catch (Exception) { }
        }

        /// <summary>ثبت خروجی گرفتن (اکسل، PDF و ...). از نظر نشت اطلاعات حساس است.</summary>
        public static void Export(string format, string? what, string? entity = null,
                                  string? entityKey = null, string? formName = null, int? rowCount = null)
        {
            try
            {
                var title = $"خروجی {format} از {what ?? "گزارش"}" +
                            (rowCount is > 0 ? $" ({rowCount} ردیف)" : string.Empty);

                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Report,
                    Severity = AuditSeverity.Sensitive,
                    Action = AuditAction.Export,
                    Entity = entity,
                    EntityKey = entityKey,
                    FormName = formName,
                    Title = title,
                    IsCritical = true,
                });
            }
            catch (Exception) { }
        }

        // ── ورود و خروج ──────────────────────────────────────────────────

        public static void Login(string? userName, string? formName = null)
            => Auth(AuditAction.Login, $"ورود کاربر {userName}", true, formName);

        public static void LoginFailed(string? userName, string? reason = null, string? formName = null)
            => Auth(AuditAction.LoginFailed,
                    $"ورود ناموفق {userName}" + (string.IsNullOrWhiteSpace(reason) ? "" : $" — {reason}"),
                    false, formName, userName);

        public static void Logout(string? userName, string? formName = null)
            => Auth(AuditAction.Logout, $"خروج کاربر {userName}", true, formName, userName);

        private static void Auth(string action, string title, bool success, string? formName,
                                 string? userNameOverride = null)
        {
            try
            {
                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Auth,
                    Severity = AuditSeverity.Sensitive,
                    Action = action,
                    FormName = formName,
                    Title = title,
                    IsSuccess = success,
                    IsCritical = true,
                    UserNameOverride = userNameOverride,
                });
            }
            catch (Exception) { }
        }

        // ── امنیتی ───────────────────────────────────────────────────────

        /// <summary>ثبت رویداد امنیتی: تغییر رمز، تغییر سطح دسترسی، مشاهده‌ی خود سوابق.</summary>
        public static void Security(string action, string persianTitle, string? entity = null,
                                    string? entityKey = null, string? formName = null, string? detail = null)
        {
            try
            {
                Write(new AuditEventDraft
                {
                    Category = AuditCategory.Security,
                    Severity = AuditSeverity.Sensitive,
                    Action = action,
                    Entity = entity,
                    EntityKey = entityKey,
                    FormName = formName,
                    Title = persianTitle,
                    Detail = detail,
                    IsCritical = true,
                });
            }
            catch (Exception) { }
        }

        // ── مسیر عمومی ───────────────────────────────────────────────────

        /// <summary>
        /// ثبت یک رویداد دلخواه. برای مواردی که متد اختصاصی ندارند.
        /// جدول‌های سابقه‌ی قدیمی هم از همین مسیر پر می‌شوند.
        /// </summary>
        public static void Write(AuditEventDraft draft)
        {
            try
            {
                // عمداً اینجا روی IsRunning بازگشت زودهنگام نمی‌کنیم: تصمیم
                // درباره‌ی رویدادهای حساسِ خارج از بازه‌ی کارِ موتور با
                // AuditService.Enqueue است که آن‌ها را روی دیسک نگه می‌دارد.
                if (!AuditService.IsRunning && !draft.IsCritical) return;

                var session = AuditService.CurrentSession;
                var now = DateTime.Now;

                AuditService.Enqueue(new AuditEvent
                {
                    SessionId = session?.SessionId ?? Guid.Empty,
                    Seq = AuditService.NextSeq(),

                    // هویت در همین لحظه و روی همین نخ خوانده می‌شود، نه بعداً
                    // روی نخ پس‌زمینه؛ وگرنه اگر کاربر عوض شود رویداد به نام
                    // شخص اشتباه ثبت می‌شود.
                    UserId = session?.UserId,
                    UserName = draft.UserNameOverride ?? session?.UserName,

                    AtClient = now,
                    DateS = ToPersianDateNumber(now),
                    TimeS = ToTimeNumber(now),

                    Category = draft.Category,
                    Severity = draft.Severity,
                    Action = draft.Action,
                    Entity = draft.Entity,
                    EntityKey = draft.EntityKey,
                    FormName = draft.FormName,
                    Title = draft.Title,
                    Detail = draft.Detail,
                    IsSuccess = draft.IsSuccess,
                    ErrorMessage = draft.ErrorMessage,
                    DurationMs = draft.DurationMs,
                    CorrelationId = draft.CorrelationId,
                    IsCritical = draft.IsCritical,
                    Legacy = draft.Legacy,
                    LegacyOldValue = draft.LegacyOldValue,
                    LegacyNewValue = draft.LegacyNewValue,
                });
            }
            catch (Exception) { }
        }

        // ── کمکی‌ها ──────────────────────────────────────────────────────

        /// <summary>یک روز و معادل شمسی‌اش. تعویضش اتمیک است چون مرجع جایگزین می‌شود.</summary>
        private sealed class DayCacheEntry
        {
            public long DayTicks;
            public int DateS;
        }

        private static DayCacheEntry? _dayCache;

        /// <summary>
        /// تاریخ شمسی عددی، مثل 14050517.
        ///
        /// نتیجه به‌ازای هر روز کش می‌شود. اندازه‌گیری نشان داد سه فراخوانی
        /// <see cref="PersianCalendar"/> حدود ۲۵ میکروثانیه طول می‌کشد — که
        /// روی نخ رابط کاربری و به‌ازای <b>هر</b> رویداد، تنها هزینه‌ی محسوس
        /// کل مسیر ثبت بود (بیش از ۷۵٪ آن). تاریخ شمسی روزی یک بار عوض
        /// می‌شود، پس محاسبه‌ی دوباره‌اش برای هر رویداد اتلاف محض است.
        ///
        /// رقابت دو نخ بی‌ضرر است: هر دو همان مقدار را حساب می‌کنند و مرجع
        /// کش را با یک نوشتنِ اتمیک عوض می‌کنند.
        /// </summary>
        public static int ToPersianDateNumber(DateTime value)
        {
            try
            {
                var dayTicks = value.Date.Ticks;

                var cache = _dayCache;
                if (cache != null && cache.DayTicks == dayTicks) return cache.DateS;

                var computed = _persian.GetYear(value) * 10000
                             + _persian.GetMonth(value) * 100
                             + _persian.GetDayOfMonth(value);

                _dayCache = new DayCacheEntry { DayTicks = dayTicks, DateS = computed };
                return computed;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>ساعت عددی، مثل 142530.</summary>
        public static int ToTimeNumber(DateTime value)
            => value.Hour * 10000 + value.Minute * 100 + value.Second;

        /// <summary>تبدیل تاریخ شمسی عددی به میلادی، برای فیلتر بازه در گزارش.</summary>
        public static DateTime? FromPersianDateNumber(int value)
        {
            try
            {
                if (value < 10000000) return null;
                var y = value / 10000;
                var m = (value / 100) % 100;
                var d = value % 100;
                if (m < 1 || m > 12 || d < 1 || d > 31) return null;
                return _persian.ToDateTime(y, m, d, 0, 0, 0, 0);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string DescribeEntity(string? entity, string? key)
        {
            if (string.IsNullOrWhiteSpace(entity)) return key ?? string.Empty;
            return string.IsNullOrWhiteSpace(key) ? entity! : $"{entity} {key}";
        }

        private static string BuildDataTitle(string action, string entity, string? key)
        {
            var verb = action switch
            {
                AuditAction.Insert => "ثبت",
                AuditAction.Update => "ویرایش",
                AuditAction.Delete => "حذف",
                _ => action,
            };
            return $"{verb} {DescribeEntity(entity, key)}";
        }

        /// <summary>
        /// تنظیم سریال‌سازی JSON برای ستون DETAIL.
        ///
        /// System.Text.Json به‌طور پیش‌فرض هر نویسه‌ی غیر ASCII را escape می‌کند،
        /// یعنی «بانک ملی» به شکل \u0628\u0627... ذخیره می‌شد و کاربر در فرم
        /// سوابق به‌جای متن فارسی، دنباله‌ی کد می‌دید.
        ///
        /// UnicodeRanges.All همه‌ی نویسه‌ها را دست‌نخورده می‌گذارد ولی نویسه‌های
        /// حساس HTML (&lt; &gt; &amp; ') همچنان escape می‌شوند، پس امن‌تر از
        /// UnsafeRelaxedJsonEscaping است.
        /// </summary>
        // public است چون Prg_UI اسمبلی جداست و شیم AuditLogger هم باید از
        // همین سیاست سریال‌سازی استفاده کند؛ با internal کامپایل نمی‌شد.
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        };

        private static string? ToJson(object? value)
        {
            if (value is null) return null;
            if (value is string s) return s;
            try { return JsonSerializer.Serialize(value, JsonOptions); }
            catch (Exception) { return null; }
        }

        private static string? BuildDiffDetail(string? oldJson, string? newJson)
        {
            if (oldJson is null && newJson is null) return null;
            try
            {
                return JsonSerializer.Serialize(new { old = oldJson, @new = newJson }, JsonOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    /// <summary>پیش‌نویس رویداد. فقط برای پر کردن مقادیر اختیاری استفاده می‌شود.</summary>
    public sealed class AuditEventDraft
    {
        public AuditCategory Category { get; set; } = AuditCategory.Business;
        public AuditSeverity Severity { get; set; } = AuditSeverity.Normal;
        public string Action { get; set; } = string.Empty;
        public string? Entity { get; set; }
        public string? EntityKey { get; set; }
        public string? FormName { get; set; }
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public int? DurationMs { get; set; }
        public Guid? CorrelationId { get; set; }
        public bool IsCritical { get; set; }
        public AuditLegacyTarget Legacy { get; set; } = AuditLegacyTarget.None;
        public string? LegacyOldValue { get; set; }
        public string? LegacyNewValue { get; set; }

        /// <summary>
        /// نام کاربری که رویداد به او نسبت دارد، وقتی با کاربرِ نشست یکی نیست.
        /// فقط برای رویدادهای پیش از ورود لازم است: در لحظه‌ی «ورود ناموفق»
        /// هنوز نشست به کاربری وصل نشده، پس USER_NAME رویداد خالی می‌ماند و
        /// نمای خط زمانی با ISNULL آن را به کاربرِ نشست نسبت می‌داد — یعنی هر
        /// تلاش ناموفق زیر نام کسی می‌نشست که بعداً موفق وارد شده بود.
        /// </summary>
        public string? UserNameOverride { get; set; }
    }
}
