using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_UI.Functions;
using Prg_UI.HelperWins;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Prg_UI.HelperWins.Msgwin;

namespace Prg_UI.Wins.WinSetting
{
    public partial class MaterialThemSettingy : Window
    {
        #region Window Control Events
        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Btn_Max_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            Btn_Max.Content = new PackIcon
            {
                Kind = WindowState == WindowState.Maximized ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize
            };
        }
        private void Btn_Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void TitleDrawBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
                if (e.ClickCount == 2)
                {
                    Btn_Max_Click(null, null);
                }
            }
        }
        #endregion

        private readonly PaletteHelper _paletteHelper = new PaletteHelper();
        private Theme _theme;
        private bool _isDark;
        private readonly int _currentUserId;


        private System.Threading.Timer _colorUpdateTimer;
        private Color _pendingColor;
        private bool _isColorUpdatePending;

        public MaterialThemSettingy()
        {
            InitializeComponent();
            _currentUserId = Baseknow.USERCOD ?? 0;
            _ = LoadThemeAsync();
        }

        private void MyColorPicker1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //UpdatePrimaryColor();

                _pendingColor = MyColorPicker1.Color;
                UpdatePrimaryColorDebounced();
            }
        }
        private void MyColorPicker1_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            if (e.NewValue != e.OldValue)
            {
                //UpdatePrimaryColor();
            }
        }

        bool ChangeHappened = false;
        private void ThemeActivationsBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeHappened = true;
            _isDark = ThemeActivationsBtn.IsChecked ?? false;
            ApplyTheme();
        }
        private async void GENERAL_RANG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Msgwin msgwin = new Msgwin(true, "آیا از ذخیره این تِم مطمئن هستید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == false)
                {
                    return;
                }

                bool saved = await SaveThemeSettingsAsync();
                if (!saved)
                {
                    new Msgwin(false, "خطا در ذخیره تنظیمات تِم در دیتابیس").ShowDialog();
                    return;
                }

                ChangeHappened = false;

                new Msgwin(false, "ذخیره انجام شد.").ShowDialog();
            }
            catch
            {
                new Msgwin(false, "خطا در انجام علمیات ذخیره تِم").ShowDialog();
            }
        }
        private void TColory_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                _pendingColor = (Color)ColorConverter.ConvertFromString(TColory.Text);
                UpdatePrimaryColorDebounced();
            }
            catch { }
        }

        private void CNN_SETTING_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var connectionWindow = new WinConnectionChoose();
            connectionWindow.ShowDialog();
        }

        #region ThemeMethods
        private async Task LoadThemeAsync()
        {
            int? userId = _currentUserId > 0 ? _currentUserId : (int?)null;
            AppThemeSettings themeSettings = await AppThemeManager.LoadThemeSettingsAsync(userId);
            _isDark = themeSettings.IsDark;
            TColory.Text = themeSettings.PrimaryColor;
            MyColorPicker1.Color = (Color)ColorConverter.ConvertFromString(themeSettings.PrimaryColor);
            ThemeActivationsBtn.IsChecked = _isDark;
            ApplyTheme();
        }

        private async Task<bool> SaveThemeSettingsAsync()
        {
            int? userId = _currentUserId > 0 ? _currentUserId : (int?)null;
            return await AppThemeManager.SaveThemeSettingsAsync(_isDark, MyColorPicker1.Color.ToString(), userId);
        }


        private void LoadTheme_0()
        {
            if (CL_LMethods.MYTHEME != null)
            {
                MyColorPicker1.Color = CL_LMethods.MYTHEME.PrimaryMid.Color;
                //_isDark = Convert.ToBoolean(File.ReadAllText($@"{themeDirectory}Dark.txt"));
                ThemeActivationsBtn.IsChecked = _isDark;
            }
        }
        private void UpdatePrimaryColorDebounced()
        {
            if (_isColorUpdatePending)
                return;

            _isColorUpdatePending = true;

            _colorUpdateTimer?.Dispose();
            _colorUpdateTimer = new System.Threading.Timer(
                (_) =>
                {
                    _isColorUpdatePending = false;
                    UpdatePrimaryColorAsync(_pendingColor);
                },
                null,
                100,
                Timeout.Infinite);
        }
        private async System.Threading.Tasks.Task UpdatePrimaryColorAsync(Color newColor)
        {
            // Get the current theme
            _theme = _paletteHelper.GetTheme();

            // Update the primary color on the UI thread
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Set the new primary color
                _theme.SetPrimaryColor(newColor);

                // Apply the updated theme
                _paletteHelper.SetTheme(_theme);

                // Update the TextBox value
                TColory.Text = newColor.ToString();
            });
        }

        // Start or restart the timer for delayed color updates
        private void UpdatePrimaryColor(Color color)
        {
            _theme = _paletteHelper.GetTheme();
            _theme.SetPrimaryColor(color);
            _paletteHelper.SetTheme(_theme);
        }
        private void UpdatePrimaryColor()
        {
            _theme = _paletteHelper.GetTheme();
            _theme.SetPrimaryColor(MyColorPicker1.Color);
            _paletteHelper.SetTheme(_theme);
        }
        private void ApplyTheme()
        {
            AppThemeManager.ApplyTheme(new AppThemeSettings
            {
                IsDark = _isDark,
                PrimaryColor = MyColorPicker1.Color.ToString()
            });
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ChangeHappened)
            {
                var MSGCAP = new MSGCAPTIONMODEL() { YES_CAPTION = "برگرد", NO_CAPTION = "خارج شو" };
                Msgwin msgwin = new Msgwin(true, "اطلاعات را ذخیره نکرده اید آیا مایل به بازگشت هستید ؟", default, default, MSGCAP); msgwin.ShowDialog();
                if (msgwin.DialogResult is true)
                {
                    e.Cancel = true;
                    return;
                }
            }

            try
            {
                _colorUpdateTimer?.Dispose();
                _colorUpdateTimer = null;
            }
            catch (Exception) { }
        }

        private async void BTN_RESET_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Msgwin msgwin = new Msgwin(true, "آیا از Reset (باز نشانی تِم) کردن مطمئن هستید ؟");
                msgwin.ShowDialog();
                if (msgwin.DialogResult == false)
                {
                    return;
                }

                _isDark = false;
                TColory.Text = AppThemeSettings.DefaultPrimaryColor;
                MyColorPicker1.Color = (Color)ColorConverter.ConvertFromString(TColory.Text);
                ThemeActivationsBtn.IsChecked = _isDark;
                ApplyTheme();

                bool saved = await SaveThemeSettingsAsync();
                if (!saved)
                {
                    new Msgwin(false, "خطا در ذخیره Reset تِم در دیتابیس").ShowDialog();
                    return;
                }

                ChangeHappened = false;
            }
            catch
            {
                new Msgwin(false, "خطا در انجام علمیات Reset تِم").ShowDialog();
            }
        }
    }
}
