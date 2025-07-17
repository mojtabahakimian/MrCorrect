using MaterialDesignThemes.Wpf;
using Prg_Proccessy.MODELS;
using Prg_Proccessy.SQLMODELS;
using Prg_UI.Wins.WinMenus.MANAGE_DASHBOARD.BUDGET;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
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

        public int PASSED_IDNUM { get; set; }

        private bool _isPersiannotify;
        public bool IsPersianNotify
        {
            set
            {
                _isPersiannotify = value;

                if (_isPersiannotify)
                {
                    MATN_PANEL.FlowDirection = FlowDirection.RightToLeft;
                }
                else
                {
                    MATN_PANEL.FlowDirection = FlowDirection.LeftToRight;
                }
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

            var workingArea = SystemParameters.WorkArea;
            double bottomOffset = 10; // Start with a small offset from the bottom

            foreach (var notification in _activeNotifications)
            {
                bottomOffset += notification.ActualHeight + 10;
            }

            Left = workingArea.Right - Width - 10;
            Top = workingArea.Bottom - Height - bottomOffset;

            Show();
            AnimateIn();

            _timer.Interval = TimeSpan.FromSeconds(durationInSeconds);
            _timer.Start();

            _activeNotifications.Add(this);

            if (_activeNotifications.Count > 1)
            {
                ShowHideAllButton();
            }

            // Improve performance by disabling unnecessary rendering
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }
        }
        private void RepositionNotifications()
        {
            double bottomOffset = 10;
            var storyboard = new Storyboard();

            foreach (var notification in _activeNotifications)
            {
                var slideUp = new DoubleAnimation
                {
                    From = notification.Top,
                    To = SystemParameters.WorkArea.Bottom - notification.Height - bottomOffset,
                    Duration = TimeSpan.FromSeconds(0.5)
                };

                Storyboard.SetTarget(slideUp, notification);
                Storyboard.SetTargetProperty(slideUp, new PropertyPath(Window.TopProperty));

                storyboard.Children.Add(slideUp);
                bottomOffset += notification.ActualHeight + 10;
            }

            storyboard.Begin();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            Dispatcher.Invoke(AnimateOut); // Ensure AnimateOut runs on the UI thread
        }

        private void AnimateIn()
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            BeginAnimation(OpacityProperty, fadeIn);
        }

        private void AnimateOut()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            fadeOut.Completed += (s, a) =>
            {
                _timer.Tick -= Timer_Tick; // Unsubscribe from the event
                _activeNotifications.Remove(this);
                Close();
                if (_activeNotifications.Count <= 1)
                {
                    HideHideAllButton();
                }
                else
                {
                    _hideAllButton?.UpdatePosition();
                }
                RepositionNotifications();
            };
            BeginAnimation(OpacityProperty, fadeOut);

            //RepositionNotifications();
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
                //IBX.Owner = this;
                //IBX.Topmost = false;
                //IBX.WindowState = WindowState.Minimized;
                IBX.Show();
            }
        }
    }
}
