using MaterialDesignThemes.Wpf;
using Prg_Proccessy.SQLMODELS;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Prg_UI.Functions
{
    internal sealed class AppThemeSettings
    {
        public bool IsDark { get; set; }
        public string PrimaryColor { get; set; } = DefaultPrimaryColor;

        public const string ThemeIsDarkOptionName = "Theme.IsDarkMode";
        public const string ThemePrimaryColorOptionName = "Theme.PrimaryColor";
        public const string DefaultPrimaryColor = "#03A9F4";
    }

    internal static class AppThemeManager
    {
        private static readonly PaletteHelper PaletteHelper = new PaletteHelper();
        private static readonly GeneralOptionManager GeneralOptionManager = new GeneralOptionManager();

        /// <summary>
        /// UID را در نام کلید encode می‌کند تا بدون تغییر schema، ذخیره per-user ممکن شود.
        /// مثال: "Theme.IsDarkMode_U78" برای کاربر 78
        /// </summary>
        private static string BuildIsDarkKey(int userId) =>
            $"{AppThemeSettings.ThemeIsDarkOptionName}_U{userId}";

        private static string BuildPrimaryColorKey(int userId) =>
            $"{AppThemeSettings.ThemePrimaryColorOptionName}_U{userId}";

        /// <summary>
        /// تنظیمات تم را از dbo.GENERAL_OPTIONS بارگذاری می‌کند.
        /// کلید per-user: "Theme.IsDarkMode_U{userId}"
        /// اگر رکورد نباشد یا userId معتبر نباشد، مقادیر پیش‌فرض برگشت داده می‌شود.
        /// </summary>
        public static async Task<AppThemeSettings> LoadThemeSettingsAsync(int? userId = null)
        {
            var themeSettings = new AppThemeSettings();

            if (!(userId > 0))
            {
                return themeSettings;
            }

            string isDarkKey = BuildIsDarkKey(userId.Value);
            string colorKey = BuildPrimaryColorKey(userId.Value);

            try
            {
                // userId=null تا فیلتر UID اعمال نشود؛ کلید خودش per-user است
                var options = await GeneralOptionManager
                    .GetOptionsAsync(new[] { isDarkKey, colorKey }, userId: null)
                    .ConfigureAwait(false);

                string darkModeDb = options.FirstOrDefault(x => x.OptionName == isDarkKey)?.OptionValue;
                string primaryColorDb = options.FirstOrDefault(x => x.OptionName == colorKey)?.OptionValue;

                if (!string.IsNullOrWhiteSpace(darkModeDb) && bool.TryParse(darkModeDb, out bool isDark))
                {
                    themeSettings.IsDark = isDark;
                }

                if (!string.IsNullOrWhiteSpace(primaryColorDb))
                {
                    themeSettings.PrimaryColor = primaryColorDb;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppThemeManager] خطا در بارگذاری تنظیمات تم از دیتابیس: {ex.Message}");
            }

            return themeSettings;
        }

        public static void ApplyTheme(AppThemeSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            Theme theme = PaletteHelper.GetTheme();
            ThemeExtensions.SetBaseTheme(theme, settings.IsDark ? BaseTheme.Dark : BaseTheme.Light);
            theme.SetPrimaryColor(ParseColor(settings.PrimaryColor));
            PaletteHelper.SetTheme(theme);
        }

        /// <summary>
        /// تنظیمات تم را در dbo.GENERAL_OPTIONS با کلید per-user ذخیره می‌کند.
        /// کلید: "Theme.IsDarkMode_U{userId}" - بدون نیاز به تغییر schema دیتابیس.
        /// </summary>
        public static async Task<bool> SaveThemeSettingsAsync(bool isDark, string primaryColor, int? userId = null)
        {
            if (!(userId > 0))
            {
                return false;
            }

            string colorValue = string.IsNullOrWhiteSpace(primaryColor)
                ? AppThemeSettings.DefaultPrimaryColor
                : primaryColor;

            var darkModeOption = new GENERAL_OPTIONS
            {
                OptionName = BuildIsDarkKey(userId.Value),
                OptionValue = isDark.ToString(),
                Description = $"Material Theme Dark Mode - User {userId}"
            };

            var primaryColorOption = new GENERAL_OPTIONS
            {
                OptionName = BuildPrimaryColorKey(userId.Value),
                OptionValue = colorValue,
                Description = $"Material Theme Primary Color - User {userId}"
            };

            try
            {
                // userId=null: MERGE بر اساس OptionName (که خودش per-user است) match می‌کند
                bool darkModeSaved = await GeneralOptionManager.SaveOptionAsync(darkModeOption, userId: null).ConfigureAwait(false);
                bool primaryColorSaved = await GeneralOptionManager.SaveOptionAsync(primaryColorOption, userId: null).ConfigureAwait(false);
                return darkModeSaved && primaryColorSaved;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppThemeManager] خطا در ذخیره تنظیمات تم در دیتابیس: {ex.Message}");
                return false;
            }
        }

        private static Color ParseColor(string colorValue)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(string.IsNullOrWhiteSpace(colorValue)
                    ? AppThemeSettings.DefaultPrimaryColor
                    : colorValue);
            }
            catch
            {
                return (Color)ColorConverter.ConvertFromString(AppThemeSettings.DefaultPrimaryColor);
            }
        }
    }
}
