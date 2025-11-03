using Prg_Proccessy.SQLMODELS;
using Prg_SendInvoice.CNNMANAGER;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prg_UI.Functions
{
    public class GeneralOptionManager
    {
        private readonly CL_CCNNMANAGER _dbms;
        private static bool? _cachedIsRDPMode;
        private static readonly object _lockObject = new object();
        private const string RDP_MODE_OPTION_NAME = "IsRDPMode";

        public GeneralOptionManager()
        {
            _dbms = new CL_CCNNMANAGER();
        }

        /// <summary>
        /// مقدار IsRDPMode را از کش یا دیتابیس دریافت می‌کند.
        /// </summary>
        public bool IsRDPMode
        {
            get
            {
                if (_cachedIsRDPMode.HasValue)
                {
                    return _cachedIsRDPMode.Value;
                }

                // اگر کش نشده باشد، از دیتابیس بخوانیم
                var task = GetOptionAsync(RDP_MODE_OPTION_NAME);
                task.Wait();
                var option = task.Result;

                bool value = false;
                if (option != null && !string.IsNullOrWhiteSpace(option.OptionValue))
                {
                    bool.TryParse(option.OptionValue, out value);
                }

                lock (_lockObject)
                {
                    _cachedIsRDPMode = value;
                }

                return value;
            }
            set
            {
                lock (_lockObject)
                {
                    _cachedIsRDPMode = value;
                }

                // ذخیره در دیتابیس به صورت async
                var option = new GENERAL_OPTIONS
                {
                    OptionName = RDP_MODE_OPTION_NAME,
                    OptionValue = value.ToString(),
                    Description = "Remote Desktop Performance Mode - حالت بهینه‌سازی عملکرد برای Remote Desktop"
                };

                var task = SaveOptionAsync(option);
                task.Wait();
            }
        }

        /// <summary>
        /// مقدار IsRDPMode را به صورت async دریافت می‌کند.
        /// </summary>
        public async Task<bool> GetIsRDPModeAsync()
        {
            if (_cachedIsRDPMode.HasValue)
            {
                return _cachedIsRDPMode.Value;
            }

            var option = await GetOptionAsync(RDP_MODE_OPTION_NAME);
            bool value = false;
            if (option != null && !string.IsNullOrWhiteSpace(option.OptionValue))
            {
                bool.TryParse(option.OptionValue, out value);
            }

            lock (_lockObject)
            {
                _cachedIsRDPMode = value;
            }

            return value;
        }

        /// <summary>
        /// مقدار IsRDPMode را به صورت async ذخیره می‌کند.
        /// </summary>
        public async Task<bool> SetIsRDPModeAsync(bool value)
        {
            lock (_lockObject)
            {
                _cachedIsRDPMode = value;
            }

            var option = new GENERAL_OPTIONS
            {
                OptionName = RDP_MODE_OPTION_NAME,
                OptionValue = value.ToString(),
                Description = "Remote Desktop Performance Mode - حالت بهینه‌سازی عملکرد برای Remote Desktop"
            };

            return await SaveOptionAsync(option);
        }

        /// <summary>
        /// تنظیمات کلی را از دیتابیس بارگذاری و کش می‌کند. این متد باید در ابتدای برنامه فراخوانی شود.
        /// </summary>
        public async Task InitializeAsync()
        {
            await GetIsRDPModeAsync();
        }

        /// <summary>
        /// کش تنظیمات را پاک می‌کند.
        /// </summary>
        public static void ClearCache()
        {
            lock (_lockObject)
            {
                _cachedIsRDPMode = null;
            }
        }

        /// <summary>
        /// یک تنظیم خاص را بر اساس نام آن از پایگاه داده بازیابی می‌کند.
        /// </summary>
        /// <param name="optionName">نام منحصر به فرد تنظیم.</param>
        /// <returns>یک شیء از نوع GENERAL_OPTIONS یا در صورت عدم وجود، null را برمی‌گرداند.</returns>
        public async Task<GENERAL_OPTIONS?> GetOptionAsync(string optionName)
        {
            if (string.IsNullOrWhiteSpace(optionName))
            {
                throw new ArgumentException("نام تنظیم نمی‌تواند خالی باشد.", nameof(optionName));
            }

            const string sql = "SELECT * FROM dbo.GENERAL_OPTIONS WHERE OptionName = @OptionName;";

            try
            {
                var result = await _dbms.SqlQueryAsync<GENERAL_OPTIONS>(sql, new { OptionName = optionName });
                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // در اینجا می‌توانید خطا را لاگ کنید
                Console.WriteLine($"خطا در بازیابی تنظیمات: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// چندین تنظیم را بر اساس لیست نام آن‌ها از پایگاه داده بازیابی می‌کند.
        /// </summary>
        /// <param name="optionNames">لیستی از نام‌های منحصر به فرد تنظیمات.</param>
        /// <returns>لیستی از اشیاء GENERAL_OPTIONS یافت شده را برمی‌گرداند.</returns>
        public async Task<List<GENERAL_OPTIONS>> GetOptionsAsync(IEnumerable<string> optionNames)
        {
            if (optionNames == null || !optionNames.Any())
            {
                return new List<GENERAL_OPTIONS>();
            }

            const string sql = "SELECT * FROM dbo.GENERAL_OPTIONS WHERE OptionName IN @OptionNames;";

            try
            {
                var result = await _dbms.SqlQueryAsync<GENERAL_OPTIONS>(sql, new { OptionNames = optionNames });
                return result.ToList();
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                Console.WriteLine($"خطا در بازیابی گروهی تنظیمات: {ex.Message}");
                return new List<GENERAL_OPTIONS>(); // بازگرداندن لیست خالی در صورت بروز خطا
            }
        }

        /// <summary>
        /// یک تنظیم را در پایگاه داده ذخیره می‌کند. اگر تنظیم وجود داشته باشد، آن را بروزرسانی (UPDATE) و در غیر این صورت، آن را درج (INSERT) می‌کند.
        /// </summary>
        /// <param name="option">شیء تنظیم برای ذخیره‌سازی.</param>
        /// <returns>در صورت موفقیت عملیات، true و در غیر این صورت false را برمی‌گرداند.</returns>
        public async Task<bool> SaveOptionAsync(GENERAL_OPTIONS option)
        {
            if (option == null || string.IsNullOrWhiteSpace(option.OptionName))
            {
                throw new ArgumentException("شیء تنظیم یا نام آن نمی‌تواند خالی باشد.");
            }

            const string sql = @"
            MERGE dbo.GENERAL_OPTIONS AS target
            USING (SELECT @OptionName AS OptionName) AS source
            ON (target.OptionName = source.OptionName)
            WHEN MATCHED THEN
                UPDATE SET 
                    OptionValue = @OptionValue, 
                    Description = @Description, 
                    LastUpdated = GETDATE()
            WHEN NOT MATCHED THEN
                INSERT (OptionName, OptionValue, Description)
                VALUES (@OptionName, @OptionValue, @Description);";

            try
            {
                var parameters = new
                {
                    option.OptionName,
                    option.OptionValue,
                    option.Description
                };

                int? affectedRows = await _dbms.ExecuteSqlCommandAsync(sql, parameters);
                return affectedRows.HasValue && affectedRows > 0;
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                Console.WriteLine($"خطا در ذخیره‌سازی تنظیمات: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// یک تنظیم خاص را بر اساس نام آن از پایگاه داده حذف می‌کند.
        /// </summary>
        /// <param name="optionName">نام منحصر به فرد تنظیم برای حذف.</param>
        /// <returns>در صورت موفقیت عملیات حذف، true و در غیر این صورت false را برمی‌گرداند.</returns>
        public async Task<bool> DeleteOptionAsync(string optionName)
        {
            if (string.IsNullOrWhiteSpace(optionName))
            {
                throw new ArgumentException("نام تنظیم نمی‌تواند خالی باشد.", nameof(optionName));
            }

            const string sql = "DELETE FROM dbo.GENERAL_OPTIONS WHERE OptionName = @OptionName;";

            try
            {
                int? affectedRows = await _dbms.ExecuteSqlCommandAsync(sql, new { OptionName = optionName });
                return affectedRows.HasValue && affectedRows > 0;
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                Console.WriteLine($"خطا در حذف تنظیمات: {ex.Message}");
                return false;
            }
        }
    }
}
