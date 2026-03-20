using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prg_UI.Functions
{
    public partial class MdiWindowHost : UserControl
    {
        private Window? _hostedWindow;
        private bool _isDragging;
        private Point _dragStartOffset;
        private bool _isMaximized;
        private bool _isMinimized;
        private double _restoreLeft, _restoreTop, _restoreWidth, _restoreHeight;

        public event EventHandler? CloseRequested;

        public MdiWindowHost()
        {
            InitializeComponent();

            // بستن پنجره با Ctrl+F4
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.F4 && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    e.Handled = true;
                    CloseHost();
                }
            };
        }

        public void SetTitle(string title)
        {
            TitleText.Text = title;
        }

        public bool IsHosting(Window window) => _hostedWindow == window;

        public void HostWindow(Window window)
        {
            _hostedWindow = window;

            if (!string.IsNullOrEmpty(window.Title))
                TitleText.Text = window.Title;

            // انتقال محتوای پنجره به MDI host
            var content = window.Content as UIElement;
            window.Content = null;

            // انتقال DataContext
            if (content is FrameworkElement fe && window.DataContext != null)
                fe.DataContext = window.DataContext;

            // ادغام ResourceDictionary پنجره در host تا binding ها کار کنند
            if (window.Resources != null && window.Resources.Count > 0)
            {
                Resources.MergedDictionaries.Add(window.Resources);
            }

            ContentArea.Content = content;

            // وقتی پنجره بسته شد، host را از canvas حذف کن
            window.Closed += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        public void CloseHost()
        {
            if (_hostedWindow != null)
            {
                try
                {
                    // بستن پنجره اصلی باعث trigger شدن Closed event می‌شود
                    // که CloseRequested را صدا می‌زند
                    _hostedWindow.Close();
                    return;
                }
                catch { }
            }
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseHost();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            if (!_isMinimized)
            {
                // ذخیره ارتفاع و مخفی کردن محتوا
                _restoreHeight = ActualHeight;
                ContentArea.Visibility = Visibility.Collapsed;
                Height = 36;
                _isMinimized = true;

                BtnMinimize.Content = new MaterialDesignThemes.Wpf.PackIcon
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore,
                    Width = 14, Height = 14
                };
                BtnMinimize.ToolTip = "بازگردانی";
            }
            else
            {
                // بازگردانی
                ContentArea.Visibility = Visibility.Visible;
                Height = _restoreHeight > 0 ? _restoreHeight : double.NaN;
                _isMinimized = false;

                BtnMinimize.Content = new MaterialDesignThemes.Wpf.PackIcon
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMinimize,
                    Width = 14, Height = 14
                };
                BtnMinimize.ToolTip = "کوچک کردن";
            }
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            var canvas = Parent as Canvas;
            if (canvas == null) return;

            if (!_isMaximized)
            {
                // ذخیره وضعیت فعلی
                _restoreLeft = Canvas.GetLeft(this);
                _restoreTop = Canvas.GetTop(this);
                _restoreWidth = ActualWidth;
                _restoreHeight = ActualHeight;

                // پر کردن کل canvas
                Canvas.SetLeft(this, 0);
                Canvas.SetTop(this, 0);
                Width = canvas.ActualWidth;
                Height = canvas.ActualHeight;
                _isMaximized = true;

                // اطمینان از نمایش محتوا اگر minimize شده بود
                if (_isMinimized)
                {
                    ContentArea.Visibility = Visibility.Visible;
                    _isMinimized = false;
                }

                BtnMaximize.Content = new MaterialDesignThemes.Wpf.PackIcon
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore,
                    Width = 14, Height = 14
                };
                BtnMaximize.ToolTip = "بازگردانی";
            }
            else
            {
                // بازگردانی به وضعیت قبلی
                Canvas.SetLeft(this, _restoreLeft);
                Canvas.SetTop(this, _restoreTop);
                Width = _restoreWidth;
                Height = _restoreHeight;
                _isMaximized = false;

                BtnMaximize.Content = new MaterialDesignThemes.Wpf.PackIcon
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize,
                    Width = 14, Height = 14
                };
                BtnMaximize.ToolTip = "بزرگ کردن";
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                BtnMaximize_Click(sender, new RoutedEventArgs());
                return;
            }

            _isDragging = true;
            var canvas = Parent as Canvas;
            if (canvas != null)
            {
                _dragStartOffset = e.GetPosition(canvas);
                _dragStartOffset.X -= Canvas.GetLeft(this);
                _dragStartOffset.Y -= Canvas.GetTop(this);
            }
            TitleBar.CaptureMouse();
            BringToFront();
            e.Handled = true;
        }

        private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            TitleBar.ReleaseMouseCapture();
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            var canvas = Parent as Canvas;
            if (canvas == null) return;

            var pos = e.GetPosition(canvas);
            double newLeft = pos.X - _dragStartOffset.X;
            double newTop = pos.Y - _dragStartOffset.Y;

            // محدود کردن به داخل canvas
            newLeft = Math.Max(0, Math.Min(newLeft, canvas.ActualWidth - ActualWidth));
            newTop = Math.Max(0, Math.Min(newTop, canvas.ActualHeight - 36));

            Canvas.SetLeft(this, newLeft);
            Canvas.SetTop(this, newTop);
        }

        public void BringToFront()
        {
            var canvas = Parent as Canvas;
            if (canvas == null) return;

            int maxZ = 0;
            foreach (UIElement child in canvas.Children)
            {
                int z = Canvas.GetZIndex(child);
                if (z > maxZ) maxZ = z;
            }
            Canvas.SetZIndex(this, maxZ + 1);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            BringToFront();
        }
    }
}
