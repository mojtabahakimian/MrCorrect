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
        /// تنظیمات تم را از dbo.GENERAL_OPTIONS بر اساس کاربر بارگذاری می‌کند.
        /// اگر کاربر وجود نداشته باشد یا رکورد در دیتابیس نباشد، مقادیر پیش‌فرض برگردانده می‌شود.
        /// </summary>
        public static async Task<AppThemeSettings> LoadThemeSettingsAsync(int? userId = null)
        {
            var themeSettings = new AppThemeSettings(); // مقادیر پیش‌فرض

            if (!(userId > 0))
            {
                return themeSettings;
            }

            try
            {
                var options = await GeneralOptionManager
                    .GetOptionsAsync(
                        new[] { AppThemeSettings.ThemeIsDarkOptionName, AppThemeSettings.ThemePrimaryColorOptionName },
                        userId)
                    .ConfigureAwait(false);

                string darkModeDb = options.FirstOrDefault(x => x.OptionName == AppThemeSettings.ThemeIsDarkOptionName)?.OptionValue;
                string primaryColorDb = options.FirstOrDefault(x => x.OptionName == AppThemeSettings.ThemePrimaryColorOptionName)?.OptionValue;

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
                // در صورت خطای دیتابیس، مقادیر پیش‌فرض بازگردانده می‌شود
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
        /// تنظیمات تم را فقط در dbo.GENERAL_OPTIONS ذخیره می‌کند (per-user).
        /// اگر userId معتبر نباشد، false برمی‌گردد چون نمی‌توان بدون کاربر ذخیره کرد.
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
                OptionName = AppThemeSettings.ThemeIsDarkOptionName,
                OptionValue = isDark.ToString(),
                Description = "Material Theme Dark Mode"
            };

            var primaryColorOption = new GENERAL_OPTIONS
            {
                OptionName = AppThemeSettings.ThemePrimaryColorOptionName,
                OptionValue = colorValue,
                Description = "Material Theme Primary Color"
            };

            try
            {
                bool darkModeSaved = await GeneralOptionManager.SaveOptionAsync(darkModeOption, userId).ConfigureAwait(false);
                bool primaryColorSaved = await GeneralOptionManager.SaveOptionAsync(primaryColorOption, userId).ConfigureAwait(false);
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
