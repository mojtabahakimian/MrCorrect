using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;

namespace UiTools
{
    public class NotificationInfo
    {
        public string Title { get; }
        public string Message { get; }
        public PackIconKind IconKind { get; }
        public double Duration { get; }
        public bool IsPersian { get; }
        public int IDNUM { get; set; }

        public NotificationInfo(string title, string message, PackIconKind iconKind, double duration, bool isPersian, int idnum)
        {
            Title = title;
            Message = message;
            IconKind = iconKind;
            Duration = duration;
            IsPersian = isPersian;
            IDNUM = idnum;
        }
    }
    public class NotificationManager
    {
        private static readonly Lazy<NotificationManager> _instance = new Lazy<NotificationManager>(() => new NotificationManager());

        public static NotificationManager Instance => _instance.Value;

        private readonly Queue<NotificationInfo> _notificationQueue = new Queue<NotificationInfo>();
        private const int MaxSimultaneousNotifications = 10;
        private const int MaxQueuedNotifications = 100;
        private int _activeNotifications = 0;


        public void EnqueueNotification(string title, string message, PackIconKind iconKind, double durationInSeconds, bool isPersian = true, int IDNUM = 0)
        {

            if (_notificationQueue.Count >= MaxQueuedNotifications)
            {
                // Consider logging this event or notifying the user
                return;
            }

            var notificationInfo = new NotificationInfo(title, message, iconKind, durationInSeconds, isPersian, IDNUM);
            _notificationQueue.Enqueue(notificationInfo);

            if (_activeNotifications < MaxSimultaneousNotifications)
            {
                ShowNextNotification();
            }
        }

        private void ShowNextNotification()
        {
            if (_notificationQueue.Count == 0 || _activeNotifications >= MaxSimultaneousNotifications)
            {
                return;
            }

            var info = _notificationQueue.Dequeue();
            _activeNotifications++;

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var notification = new NotificationWindow(info.IsPersian);
                notification.Closed += (s, e) =>
                {
                    _activeNotifications--;
                    ShowNextNotification();
                };
                notification.ShowNotification(info.Title, info.Message, info.IconKind, info.Duration, info.IDNUM);
            });
        }

        public void ExampleUsage()
        {
            NotificationManager.Instance.EnqueueNotification(
                "System Update",
                "A new system update is available. Please restart your computer.",
                PackIconKind.Update,
                8);


            // First notification
            var notification1 = new NotificationWindow(isPersiannotify: false);
            notification1.ShowNotification(
                title: "First Notification",
                message: "This is the first notification.",
                iconKind: PackIconKind.InformationOutline,
                durationInSeconds: 5, default);
        }
    }
}
