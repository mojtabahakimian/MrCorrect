using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prg_UI.Functions
{
    /// <summary>
    /// نوع منبع بروزرسانی
    /// </summary>
    public enum UpdateSourceType
    {
        /// <summary>پوشه شبکه (UNC / Share)</summary>
        Share,
        /// <summary>سرور FTP</summary>
        Ftp
    }

    /// <summary>
    /// تنظیمات آپدیت خودکار — از فایل update.config.json کنار EXE خوانده می‌شود.
    ///
    /// نمونه برای Share (UNC):
    /// {
    ///   "Type": "Share",
    ///   "SharePath": "\\\\SERVER\\ShareName\\update"
    /// }
    ///
    /// نمونه برای FTP:
    /// {
    ///   "Type": "Ftp",
    ///   "FtpHost": "ftp.example.com",
    ///   "FtpPort": 21,
    ///   "FtpPath": "/update",
    ///   "FtpUsername": "myuser",
    ///   "FtpPassword": "mypass",
    ///   "FtpPassive": true
    /// }
    /// </summary>
    public class UpdateConfig
    {
        // ── مشترک ────────────────────────────────────────────────────────────
        public UpdateSourceType Type { get; set; } = UpdateSourceType.Share;

        // ── Share / UNC ──────────────────────────────────────────────────────
        /// <summary>مسیر پوشه شبکه که فایل EXE آپدیت داخل آن است</summary>
        public string SharePath { get; set; } = @"\\win-server2016\ade\EXE\update";

        // ── FTP ──────────────────────────────────────────────────────────────
        /// <summary>آدرس هاست FTP (بدون پروتکل)</summary>
        public string FtpHost { get; set; } = string.Empty;

        /// <summary>پورت FTP؛ پیش‌فرض ۲۱</summary>
        public int FtpPort { get; set; } = 21;

        /// <summary>مسیر پوشه روی FTP که فایل EXE آپدیت داخل آن است</summary>
        public string FtpPath { get; set; } = "/";

        /// <summary>نام کاربری FTP</summary>
        public string FtpUsername { get; set; } = "anonymous";

        /// <summary>رمز عبور FTP</summary>
        public string FtpPassword { get; set; } = string.Empty;

        /// <summary>حالت Passive FTP؛ معمولاً true برای اتصالات پشت فایروال</summary>
        public bool FtpPassive { get; set; } = true;

        // ── پیش‌فرض (سازگار با مسیر قدیمی هاردکد شده) ───────────────────────
        public static readonly UpdateConfig Default = new UpdateConfig
        {
            Type = UpdateSourceType.Share,
            SharePath = @"\\win-server2016\ade\EXE\update"
        };

        private const string CONFIG_FILENAME = "update.config.json";

        // ── بارگذاری ──────────────────────────────────────────────────────────
        /// <summary>
        /// فایل update.config.json را از پوشه EXE می‌خواند.
        /// اگر فایل وجود نداشت یا خراب بود، مقدار Default را برمی‌گرداند.
        /// </summary>
        public static UpdateConfig Load(string appDir)
        {
            try
            {
                string path = Path.Combine(appDir, CONFIG_FILENAME);
                if (!File.Exists(path))
                    return Default;

                string json = File.ReadAllText(path);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                return JsonSerializer.Deserialize<UpdateConfig>(json, options) ?? Default;
            }
            catch
            {
                return Default;
            }
        }

        // ── helperها ──────────────────────────────────────────────────────────

        /// <summary>مسیر کامل UNC فایل EXE روی Share</summary>
        public string GetShareFilePath(string exeName) =>
            Path.Combine(SharePath ?? string.Empty, exeName);

        /// <summary>آدرس کامل FTP فایل EXE</summary>
        public string GetFtpUri(string exeName)
        {
            string basePath = (FtpPath ?? "/").TrimEnd('/');
            return $"ftp://{FtpHost}:{FtpPort}{basePath}/{exeName}";
        }

        /// <summary>نمایش خوانا برای پیام‌های خطا</summary>
        public string GetDisplayPath(string exeName) =>
            Type == UpdateSourceType.Share ? GetShareFilePath(exeName) : GetFtpUri(exeName);
    }
}
