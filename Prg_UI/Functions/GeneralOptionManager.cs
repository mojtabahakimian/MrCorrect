using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using System;
using System.Collections.Concurrent; // اضافه شد
using System.Collections.Generic;
using System.Linq;
using System.Threading; // اضافه شد
using System.Threading.Tasks;
using Dapper; // اضافه شد
using Microsoft.Data.SqlClient; // اضافه شد

namespace Prg_UI.Functions
{
    /// <summary>
    /// مدیریت تنظیمات کلی برنامه.
    /// این کلاس به صورت Thread-Safe طراحی شده و از Caching برای بهبود عملکرد استفاده می‌کند.
    /// </summary>
    public class GeneralOptionManager
    {
        private readonly CL_CCNNMANAGER _dbms;
        private readonly string _connectionString; // اضافه شد
        private const string RDP_MODE_OPTION_NAME = "IsRDPMode";

        // یک کش Thread-Safe برای نگهداری تنظیمات RDP به ازای هر کاربر (UserID -> RDPMode)
        // این کش استاتیک است و در طول عمر برنامه باقی می‌ماند.
        private static readonly ConcurrentDictionary<int, bool> _rdpModeCache = new ConcurrentDictionary<int, bool>();

        // یک قفل ناهمزمان (Async Lock) برای جلوگیری از فراخوانی همزمان دیتابیس هنگام پر کردن کش
        private static readonly SemaphoreSlim _cacheSemaphore = new SemaphoreSlim(1, 1);

        public GeneralOptionManager()
        {
            _dbms = new CL_CCNNMANAGER();
            _connectionString = CL_CCNNMANAGER.CONNECTION_STR; // اضافه شد
        }

        /// <summary>
        /// مقدار IsRDPMode را از کش دریافت می‌کند (Static Property - Per User).
        /// این پراپرتی بسیار سریع و بدون هنگ کردن (Deadlock-Free) است.
        /// !! مهم: این پراپرتی برای خواندن مقدار به کش متکی است.
        /// !! اطمینان حاصل کنید که متد InitializeSync() یا InitializeAsync() در ابتدای برنامه فراخوانی شده باشد.
        /// استفاده: if (GeneralOptionManager.IsRDPMode) { ... }
        /// </summary>
        public static bool IsRDPMode
        {
            get
            {
                int currentUserId = Baseknow.USERCOD ?? 0;

                // مقدار را مستقیماً از کش می‌خواند.
                // اگر در کش نباشد (یعنی InitializeSync اجرا نشده)، مقدار پیش‌فرض (false) برمی‌گرداند.
                _rdpModeCache.TryGetValue(currentUserId, out bool value);
                return value;
            }
            set
            {
                int currentUserId = Baseknow.USERCOD ?? 0;

                // 1. ابتدا کش را فوراً به‌روزرسانی می‌کنیم تا UI پاسخگو باشد.
                _rdpModeCache.AddOrUpdate(currentUserId, value, (id, oldVal) => value);

                // 2. عملیات ذخیره در دیتابیس را به صورت "Fire-and-Forget" (آتش کن و فراموش کن)
                // در یک ترد جداگانه اجرا می‌کنیم تا UI هنگ نکند.
                Task.Run(async () =>
                {
                    try
                    {
                        var manager = new GeneralOptionManager();
                        await manager.SaveRDPModeToDbAsync(value, currentUserId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GeneralOptionManager] Background save failed for RDPMode: {ex.Message}");
                    }
                });
            }
        }

        /// <summary>
        /// تنظیمات را به صورت همزمان (Synchronous) بارگذاری می‌کند.
        /// !! این متد فقط باید در زمان راه‌اندازی برنامه (Startup) استفاده شود !!
        /// این متد برای جلوگیری از ددلاک در متدهای OnStartup همزمان (non-async) طراحی شده است.
        /// </summary>
        public static void InitializeSync(int? userId = null)
        {
            int currentUserId = userId ?? Baseknow.USERCOD ?? 0;
            if (currentUserId == 0) return; // کاربری وجود ندارد

            // اگر در کش وجود دارد، کاری انجام نده
            if (_rdpModeCache.ContainsKey(currentUserId))
            {
                return;
            }

            // قفل را می‌گیریم (همزمان)
            // این کار باعث می‌شود ترد اصلی (UI) منتظر بماند
            _cacheSemaphore.Wait();
            try
            {
                // بررسی مجدد (Double-Check Lock)
                if (_rdpModeCache.ContainsKey(currentUserId))
                {
                    return;
                }

                var manager = new GeneralOptionManager();
                // متد همزمان جدید را برای خواندن از دیتابیس فراخوانی می‌کنیم
                var option = manager.GetOptionSync(RDP_MODE_OPTION_NAME, currentUserId);

                bool dbValue = false;
                if (option != null && !string.IsNullOrWhiteSpace(option.OptionValue))
                {
                    bool.TryParse(option.OptionValue, out dbValue);
                }

                _rdpModeCache.AddOrUpdate(currentUserId, dbValue, (id, oldVal) => dbValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeneralOptionManager] Failed to init RDP mode sync: {ex.Message}");
                // اجازه می‌دهیم خطا پرتاب شود تا try/catch در App.xaml.cs آن را بگیرد
                throw;
            }
            finally
            {
                _cacheSemaphore.Release();
            }
        }

        /// <summary>
        /// یک تنظیم را به صورت همزمان (Synchronous) می‌خواند.
        /// !! فقط برای استفاده در InitializeSync !!
        /// </summary>
        private GENERAL_OPTIONS? GetOptionSync(string optionName, int userId)
        {
            if (string.IsNullOrWhiteSpace(optionName))
                throw new ArgumentException("نام تنظیم نمی‌تواند خالی باشد.", nameof(optionName));

            // از کانکشن استرینگ محلی و Dapper استفاده می‌کنیم
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Connection string is not initialized in GeneralOptionManager.");

            const string sql = "SELECT * FROM dbo.GENERAL_OPTIONS WHERE OptionName = @OptionName AND UID = @UID;";
            try
            {
                // استفاده از Dapper به صورت همزمان، دقیقاً مانند App.xaml.cs
                using (var db = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
                {
                    db.Open();
                    var result = db.Query<GENERAL_OPTIONS>(sql, new { OptionName = optionName, UID = userId });
                    return result.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeneralOptionManager] خطا در بازیابی همزمان تنظیمات: {ex.Message}");
                throw; // خطا را مجدداً پرتاب می‌کنیم تا App.xaml.cs آن را مدیریت کند
            }
        }


        /// <summary>
        /// مقدار IsRDPMode را به صورت async از کش یا دیتابیس دریافت می‌کند (Static Method - Per User).
        /// این متد Thread-Safe و Concurrency-Safe است.
        /// استفاده: bool rdpMode = await GeneralOptionManager.GetIsRDPModeAsync();
        /// </summary>
        public static async Task<bool> GetIsRDPModeAsync(int? userId = null)
        {
            int currentUserId = userId ?? Baseknow.USERCOD ?? 0;

            // 1. ابتدا کش را چک می‌کنیم (سریع و بدون قفل)
            if (_rdpModeCache.TryGetValue(currentUserId, out bool cachedValue))
            {
                return cachedValue;
            }

            // 2. اگر در کش نبود، باید از دیتابیس بخوانیم.
            // از Semaphore استفاده می‌کنیم تا فقط یک ترد در آن واحد به دیتابیس دسترسی پیدا کند.
            await _cacheSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                // 3. بررسی مجدد کش (الگوی Double-Check Lock)
                if (_rdpModeCache.TryGetValue(currentUserId, out bool reCheckValue))
                {
                    return reCheckValue;
                }

                // 4. فراخوانی دیتابیس
                var manager = new GeneralOptionManager();
                var option = await manager.GetOptionAsync(RDP_MODE_OPTION_NAME, currentUserId);

                bool dbValue = false;
                if (option != null && !string.IsNullOrWhiteSpace(option.OptionValue))
                {
                    bool.TryParse(option.OptionValue, out dbValue);
                }

                // 5. ذخیره مقدار جدید در کش
                _rdpModeCache.AddOrUpdate(currentUserId, dbValue, (id, oldVal) => dbValue);
                return dbValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeneralOptionManager] Failed to get RDP mode async: {ex.Message}");
                return false; // بازگشت مقدار پیش‌فرض در صورت خطا
            }
            finally
            {
                _cacheSemaphore.Release(); // آزادسازی قفل
            }
        }

        /// <summary>
        /// مقدار IsRDPMode را به صورت async در دیتابیس و کش ذخیره می‌کند (Static Method - Per User).
        /// این روش پیشنهادی برای ذخیره تنظیمات است.
        /// استفاده: await GeneralOptionManager.SetIsRDPModeAsync(true);
        /// </summary>
        public static async Task<bool> SetIsRDPModeAsync(bool value, int? userId = null)
        {
            int currentUserId = userId ?? Baseknow.USERCOD ?? 0;
            var manager = new GeneralOptionManager();

            // 1. ابتدا در دیتابیس ذخیره می‌کنیم
            bool success = await manager.SaveRDPModeToDbAsync(value, currentUserId);

            // 2. فقط در صورت موفقیت، کش را به‌روزرسانی می‌کنیم
            if (success)
            {
                _rdpModeCache.AddOrUpdate(currentUserId, value, (id, oldVal) => value);
            }
            return success;
        }

        /// <summary>
        /// تنظیمات کلی را از دیتابیس بارگذاری و کش می‌کند. این متد باید در ابتدای برنامه فراخوانی شود (Static Method).
        /// این متد برای استفاده در App.xaml.cs که async است مناسب می‌باشد.
        /// استفاده: await GeneralOptionManager.InitializeAsync();
        /// </summary>
        public static async Task InitializeAsync(int? userId = null)
        {
            // مقدار را برای کاربر فعلی (یا کاربر مشخص شده) می‌خواند و در کش قرار می‌دهد
            int currentUserId = userId ?? Baseknow.USERCOD ?? 0;
            await GetIsRDPModeAsync(currentUserId);
        }

        /// <summary>
        /// کش IsRDPMode را پاک کرده و مقدار جدید را از دیتابیس می‌خواند (نسخه Async).
        /// استفاده: bool rdpMode = await GeneralOptionManager.RefreshAndGetIsRDPModeAsync();
        /// </summary>
        public static async Task<bool> RefreshAndGetIsRDPModeAsync(int? userId = null)
        {
            int currentUserId = userId ?? Baseknow.USERCOD ?? 0;

            // 1. مقدار قدیمی را از کش حذف می‌کنیم
            _rdpModeCache.TryRemove(currentUserId, out _);

            // 2. متد Get را فراخوانی می‌کنیم تا مقدار جدید را از دیتابیس بخواند و کش کند
            return await GetIsRDPModeAsync(currentUserId);
        }

        /// <summary>
        /// یک متد کمکی داخلی برای ذخیره RDPMode در دیتابیس.
        /// </summary>
        private async Task<bool> SaveRDPModeToDbAsync(bool value, int userId)
        {
            var option = new GENERAL_OPTIONS
            {
                OptionName = RDP_MODE_OPTION_NAME,
                OptionValue = value.ToString(),
                Description = "Remote Desktop Performance Mode - حالت بهینه‌سازی عملکرد برای Remote Desktop",
                UID = userId // UID به صراحت ست می‌شود
            };
            return await SaveOptionAsync(option, userId);
        }

        /// <summary>
        /// یک تنظیم خاص را بر اساس نام آن و کد کاربر از پایگاه داده بازیابی می‌کند (per-user).
        /// </summary>
        public async Task<GENERAL_OPTIONS?> GetOptionAsync(string optionName, int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(optionName))
            {
                throw new ArgumentException("نام تنظیم نمی‌تواند خالی باشد.", nameof(optionName));
            }
            int? currentUserId = userId;
            string extra = "";
            if (currentUserId > 0)
            {
                extra = " AND UID = @UID ";
            }
            string sql = $"SELECT * FROM dbo.GENERAL_OPTIONS WHERE OptionName IN @OptionNames {extra}";
            try
            {
                var result = await _dbms.SqlQueryAsync<GENERAL_OPTIONS>(sql, new { OptionName = optionName, UID = currentUserId })
                                       .ConfigureAwait(false); // جلوگیری از ددلاک
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در بازیابی تنظیمات: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// چندین تنظیم را بر اساس لیست نام آن‌ها و کد کاربر از پایگاه داده بازیابی می‌کند (per-user).
        /// </summary>
        public async Task<List<GENERAL_OPTIONS>> GetOptionsAsync(IEnumerable<string> optionNames, int? userId = null)
        {
            if (optionNames == null || !optionNames.Any())
            {
                return new List<GENERAL_OPTIONS>();
            }
            int? currentUserId = userId /*?? Baseknow.USERCOD ?? 0*/;
            string extra = "";
            if (currentUserId > 0)
            {
                extra = " AND UID = @UID ";
            }
            string sql = $"SELECT * FROM dbo.GENERAL_OPTIONS WHERE OptionName IN @OptionNames {extra}";
            try
            {
                var result = await _dbms.SqlQueryAsync<GENERAL_OPTIONS>(sql, new { OptionNames = optionNames, UID = currentUserId })
                                       .ConfigureAwait(false); // جلوگیری از ددلاک
                return result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در بازیابی گروهی تنظیمات: {ex.Message}");
                return new List<GENERAL_OPTIONS>();
            }
        }

        /// <summary>
        /// یک تنظیم را در پایگاه داده ذخیره می‌کند (per-user). اگر تنظیم وجود داشته باشد، آن را بروزرسانی (UPDATE) و در غیر این صورت، آن را درج (INSERT) می‌کند.
        /// </summary>
        public async Task<bool> SaveOptionAsync(GENERAL_OPTIONS option, int? userId = null)
        {
            if (option == null || string.IsNullOrWhiteSpace(option.OptionName))
            {
                throw new ArgumentException("شیء تنظیم یا نام آن نمی‌تواند خالی باشد.");
            }

            int? currentUserId = userId;

            if (currentUserId > 0)
            {
                option.UID = currentUserId;
            }

            const string sql = @"
                MERGE dbo.GENERAL_OPTIONS AS target
                USING (SELECT @OptionName AS OptionName, @UID AS UID) AS source
                ON (target.OptionName = source.OptionName
                    AND ((source.UID IS NULL AND target.UID IS NULL) OR target.UID = source.UID))
                WHEN MATCHED THEN
                    UPDATE SET
                        OptionValue = @OptionValue,
                        Description = @Description,
                        LastUpdated = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (OptionName, OptionValue, Description, UID, CRT)
                    VALUES (@OptionName, @OptionValue, @Description, @UID, GETDATE());";
            try
            {
                var parameters = new
                {
                    option.OptionName,
                    option.OptionValue,
                    option.Description,
                    UID = currentUserId
                };
                int? affectedRows = await _dbms.ExecuteSqlCommandAsync(sql, parameters)
                                              .ConfigureAwait(false); // جلوگیری از ددلاک
                return affectedRows.HasValue && affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در ذخیره‌سازی تنظیمات: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// یک تنظیم خاص را بر اساس نام آن و کد کاربر از پایگاه داده حذف می‌کند (per-user).
        /// </summary>
        public async Task<bool> DeleteOptionAsync(string optionName, int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(optionName))
            {
                throw new ArgumentException("نام تنظیم نمی‌تواند خالی باشد.", nameof(optionName));
            }
            int currentUserId = userId ?? Baseknow.USERCOD ?? 0;
            const string sql = "DELETE FROM dbo.GENERAL_OPTIONS WHERE OptionName = @OptionName AND UID = @UID;";
            try
            {
                int? affectedRows = await _dbms.ExecuteSqlCommandAsync(sql, new { OptionName = optionName, UID = currentUserId })
                                              .ConfigureAwait(false); // جلوگیری از ددلاک

                // اگر حذف موفق بود، کش را هم پاک می‌کنیم
                if (affectedRows.HasValue && affectedRows > 0 && optionName == RDP_MODE_OPTION_NAME)
                {
                    _rdpModeCache.TryRemove(currentUserId, out _);
                }
                return affectedRows.HasValue && affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در حذف تنظیمات: {ex.Message}");
                return false;
            }
        }
    }
}

