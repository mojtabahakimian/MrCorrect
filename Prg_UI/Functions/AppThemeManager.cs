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

        public static async Task<AppThemeSettings> LoadThemeSettingsAsync(int? userId = null)
        {
            var themeSettings = new AppThemeSettings
            {
                IsDark = false,
                PrimaryColor = AppThemeSettings.DefaultPrimaryColor
            };

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

                string? darkModeDb = options.FirstOrDefault(x => x.OptionName == AppThemeSettings.ThemeIsDarkOptionName)?.OptionValue;
                string? primaryColorDb = options.FirstOrDefault(x => x.OptionName == AppThemeSettings.ThemePrimaryColorOptionName)?.OptionValue;

                if (!string.IsNullOrWhiteSpace(darkModeDb) && bool.TryParse(darkModeDb, out bool isDark))
                {
                    themeSettings.IsDark = isDark;
                }

                if (!string.IsNullOrWhiteSpace(primaryColorDb))
                {
                    themeSettings.PrimaryColor = primaryColorDb;
                }
            }
            catch
            {
                // در صورت خطا از تنظیمات محلی برنامه استفاده می‌کنیم.
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

        public static void SaveLocalSettings(bool isDark, string primaryColor)
        {
            // فقط برای سازگاری نسخه‌های قبلی نگه داشته شده است.
            // مسیر اصلی ذخیره تم، جدول dbo.GENERAL_OPTIONS به ازای هر کاربر است.
            Properties.Settings.Default.IsDarkMode = isDark;
            Properties.Settings.Default.PrimaryColor = string.IsNullOrWhiteSpace(primaryColor)
                ? AppThemeSettings.DefaultPrimaryColor
                : primaryColor;
            Properties.Settings.Default.Save();
        }

        public static async Task<bool> SaveThemeSettingsAsync(bool isDark, string primaryColor, int? userId = null)
        {
            if (!(userId > 0))
            {
                return false;
            }

            var darkModeOption = new GENERAL_OPTIONS
            {
                OptionName = AppThemeSettings.ThemeIsDarkOptionName,
                OptionValue = isDark.ToString(),
                Description = "Material Theme Dark Mode"
            };

            var primaryColorOption = new GENERAL_OPTIONS
            {
                OptionName = AppThemeSettings.ThemePrimaryColorOptionName,
                OptionValue = string.IsNullOrWhiteSpace(primaryColor) ? AppThemeSettings.DefaultPrimaryColor : primaryColor,
                Description = "Material Theme Primary Color"
            };

            bool darkModeSaved = await GeneralOptionManager.SaveOptionAsync(darkModeOption, userId).ConfigureAwait(false);
            bool primaryColorSaved = await GeneralOptionManager.SaveOptionAsync(primaryColorOption, userId).ConfigureAwait(false);

            return darkModeSaved && primaryColorSaved;
        }

        private static Color ParseColor(string? colorValue)
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
