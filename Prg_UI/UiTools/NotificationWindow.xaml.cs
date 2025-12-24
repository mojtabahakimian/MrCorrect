using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Wins.WinMenus.WinAutomasion;

namespace UiTools
{
    public partial class NotificationWindow : Window
    {
        private static readonly List<NotificationWindow> _activeNotifications = new List<NotificationWindow>();

        private readonly DispatcherTimer _timer;
        private static HideAllButton _hideAllButton;
        private const double HideButtonHeight = 45;

        public int PASSED_IDNUM { get; set; }

        private bool _isPersiannotify;
        private bool _isAnimatingOut;

        public bool IsPersianNotify
        {
            set
            {
                _isPersiannotify = value;
                MATN_PANEL.FlowDirection = _isPersiannotify ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            }
        }

        public NotificationWindow(bool isPersiannotify = true)
        {
            InitializeComponent();
            IsPersianNotify = isPersiannotify;

            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
        }

        public static void ShowHideAllButton()
        {
            if (_hideAllButton == null || !_hideAllButton.IsVisible)
            {
                _hideAllButton = new HideAllButton();
                _hideAllButton.Show();
            }
            _hideAllButton.UpdatePosition();
            RepositionNotifications();
        }

        public static void HideHideAllButton()
        {
            if (_hideAllButton != null && _hideAllButton.IsVisible)
            {
                _hideAllButton.Close();
                _hideAllButton = null;
            }
        }

        public void ShowNotification(string title, string message, PackIconKind iconKind, double durationInSeconds, int IDNUM)
        {
            NotificationTitle.Text = title;
            NotificationMessage.Text = message;
            NotificationIcon.Kind = iconKind;
            PASSED_IDNUM = IDNUM;

            // محاسبهٔ جایگذاری نهایی
            var workingArea = SystemParameters.WorkArea;
            double bottomOffset = 10;

            if (_activeNotifications.Count >= 1)
                bottomOffset += HideButtonHeight + 10;

            foreach (var n in _activeNotifications)
                bottomOffset += n.ActualHeight + 10;

            double targetLeft = workingArea.Right - Width - 10;
            double targetTop = workingArea.Bottom - Height - bottomOffset;

            // حالت اولیه: کمی پایین‌تر از مقصد + نامرئی
            Left = targetLeft;
            Top = targetTop + 14;     // Slide Up فاصلهٔ شروع
            Opacity = 0;

            // نمایش و سپس انیمیشن ورودی (بدون چشمک)
            Show();
            AnimateIn(targetTop);

            _timer.Interval = TimeSpan.FromSeconds(durationInSeconds);
            _timer.Start();

            _activeNotifications.Add(this);

            if (_activeNotifications.Count > 1)
                ShowHideAllButton();

            // نکته مهم: RenderMode را به حالت پیش‌فرض می‌گذاریم (حذف SoftwareOnly) تا انیمیشن روان باشد.
            // قبلاً این خط باعث رندر نرم‌افزاری و تیک‌تیک می‌شد:
            // var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            // hwndSource?.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            // (عمداً حذف شد) :contentReference[oaicite:1]{index=1}
        }

        private static void RepositionNotifications()
        {
            double bottomOffset = 10;

            if (_activeNotifications.Count > 1)
                bottomOffset += HideButtonHeight + 10;

            var storyboard = new Storyboard();

            foreach (var notification in _activeNotifications)
            {
                double newTop = SystemParameters.WorkArea.Bottom - notification.Height - bottomOffset;

                var slideUp = new DoubleAnimation
                {
                    From = notification.Top,
                    To = newTop,
                    Duration = TimeSpan.FromMilliseconds(240),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(slideUp, notification);
                Storyboard.SetTargetProperty(slideUp, new PropertyPath(TopProperty));

                storyboard.Children.Add(slideUp);
                bottomOffset += notification.ActualHeight + 10;
            }

            storyboard.Begin();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            Dispatcher.Invoke(AnimateOut);
        }

        private void AnimateIn(double finalTop)
        {
            var sb = new Storyboard();

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeIn, this);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(OpacityProperty));
            sb.Children.Add(fadeIn);

            var slide = new DoubleAnimation
            {
                From = finalTop + 14,
                To = finalTop,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(slide, this);
            Storyboard.SetTargetProperty(slide, new PropertyPath(TopProperty));
            sb.Children.Add(slide);

            sb.Begin();
        }

        private void AnimateOut()
        {
            if (_isAnimatingOut) return;   // جلوگیری از اجرای دوباره
            _isAnimatingOut = true;

            var sb = new Storyboard();

            var fadeOut = new DoubleAnimation
            {
                From = Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(fadeOut, this);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
            sb.Children.Add(fadeOut);

            var slideDown = new DoubleAnimation
            {
                From = Top,
                To = Top + 12,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(slideDown, this);
            Storyboard.SetTargetProperty(slideDown, new PropertyPath(TopProperty));
            sb.Children.Add(slideDown);

            sb.Completed += (s, a) =>
            {
                _timer.Tick -= Timer_Tick;
                _activeNotifications.Remove(this);
                Close();

                if (_activeNotifications.Count <= 1)
                    HideHideAllButton();
                else
                    _hideAllButton?.UpdatePosition();

                RepositionNotifications();
            };

            sb.Begin();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            AnimateOut();
        }

        public static void CloseAllNotifications()
        {
            foreach (var notification in _activeNotifications.ToArray())
            {
                notification._timer.Stop();
                notification.AnimateOut();
            }
        }

        public static int ActiveNotificationsCount => _activeNotifications.Count;

        public event EventHandler NotificationClosed;
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _timer?.Stop();
            _activeNotifications?.Remove(this);
            NotificationClosed?.Invoke(this, EventArgs.Empty);
        }

        private void MATN_PANEL_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PASSED_IDNUM > 0)
            {
                INBOXPAN IBX = new INBOXPAN(PASSED_IDNUM);
                IBX.Show();
            }
        }
    }
}
