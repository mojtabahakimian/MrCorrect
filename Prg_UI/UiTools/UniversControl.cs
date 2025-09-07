using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Prg_UI.UiTools
{
    public class UniversControl
    {
        private readonly System.Windows.Threading.DispatcherTimer _timer = new();
        private Popup _currentPopup = null;
        private readonly Queue<NotificationRequest> _pendingNotifications = new();

        private static readonly BrushConverter _brushConverter = new();

        // --- سیاست‌های هوشمند ---
        private readonly TimeSpan _duplicateWindow = TimeSpan.FromMilliseconds(800); // بازه ادغام پیام‌های تکراری
        private readonly int _maxAutoExtendSeconds = 5;   // سقف افزایش خودکار زمان نمایش
        private const int MaxQueueSize = 100;
        private bool _isShowing;

        // رهگیری آخرین درخواست
        private DateTime _lastRequestAt = DateTime.MinValue;
        private string _lastKey = string.Empty;

        // رهگیری پیام فعلی برای شمارنده
        private NotificationRequest _currentRequest = null;

        private sealed class NotificationRequest
        {
            public string BaseMessage { get; }
            public string DisplayMessage => RepeatCount <= 1 ? BaseMessage : $"{BaseMessage} ×{RepeatCount}";
            public Popup Popup { get; }
            public TextBox TextBox { get; }
            public Border Border { get; }
            public string Background { get; }
            public int WaitSeconds { get; set; }
            public int RepeatCount { get; set; } = 1;
            public string Key { get; }

            public NotificationRequest(string message, Popup popup, TextBox textBox, Border border, string background, int waitSeconds)
            {
                BaseMessage = message ?? string.Empty;
                Popup = popup;
                TextBox = textBox;
                Border = border;
                Background = string.IsNullOrWhiteSpace(background) ? "#E5EC2B2B" : background;
                WaitSeconds = Math.Max(1, waitSeconds);

                // کلید یکتای پیام برای ادغام: متن + رنگ + شناسه پاپ‌آپ
                Key = $"{BaseMessage}||{Background}||{(Popup != null ? Popup.GetHashCode() : 0)}";
            }
        }

        public enum RangPop { Red, Green, Blue, Yellow }

        /// <summary>
        /// Blue  : #FF007ACC
        /// Red   : #E5EC2B2B
        /// Green : #FF1AAA2C
        /// Yellow: #FFDC9E18
        /// </summary>
        public void PopNotifyShow(string msg, Popup popup = null, TextBox text = null, Border border = null, string back = "#E5EC2B2B", int waitSeconds = 2)
        {
            var request = new NotificationRequest(msg, popup, text, border, back, waitSeconds);
            var now = DateTime.Now;

            // 1) اگر همین پیام در بازه _duplicateWindow دوباره آمد → ادغام
            if (request.Key == _lastKey && (now - _lastRequestAt) <= _duplicateWindow)
            {
                // اگر همین الان همان پیام درحال نمایش است → شمارنده + تمدید تایمر
                if (_isShowing && _currentRequest != null && _currentRequest.Key == request.Key)
                {
                    _currentRequest.RepeatCount++;
                    UpdateCurrentVisual(_currentRequest);
                    ResetTimer(AddSecondsCapped(_currentRequest.WaitSeconds, 1, _maxAutoExtendSeconds));
                }
                // اگر در صف هم نمونه‌ای هست، نیازی به افزودن دوباره نیست
                return;
            }

            _lastKey = request.Key;
            _lastRequestAt = now;

            // 2) اگر درحال نمایشیم:
            if (_isShowing)
            {
                // صف را کوتاه و تازه نگه می‌داریم: فقط آخرین پیام مهم است
                if (_pendingNotifications.Count >= MaxQueueSize) _pendingNotifications.Clear();
                _pendingNotifications.Clear(); // drop intermediate backlog
                _pendingNotifications.Enqueue(request);
                return;
            }

            // 3) در غیر این صورت، مستقیم نمایش بده
            ShowPopup(request);
        }

        public void PopNotifyShowUp(string msg, Popup popup = null, TextBox text = null, Border border = null, RangPop rang = RangPop.Red, int waitSeconds = 2)
        {
            string color = rang switch
            {
                RangPop.Blue => "#FF007ACC",
                RangPop.Green => "#FF1AAA2C",
                RangPop.Yellow => "#FFDC9E18",
                _ => "#E5EC2B2B"
            };
            PopNotifyShow(msg, popup, text, border, color, waitSeconds);
        }

        // ---------- منطق داخلی ----------
        private void ShowPopup(NotificationRequest request)
        {
            _isShowing = true;
            _currentRequest = request;
            _currentPopup = request.Popup;

            // اعمال رنگ و متن (با شمارنده در صورت نیاز)
            if (request.Border != null)
            {
                try { request.Border.Background = (Brush)_brushConverter.ConvertFrom(request.Background); } catch { /* ignore */ }
            }
            if (request.TextBox != null)
            {
                request.TextBox.Text = request.DisplayMessage;
            }

            if (_currentPopup != null)
            {
                _currentPopup.IsOpen = true;
            }

            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(request.WaitSeconds);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;

            if (_currentPopup != null)
            {
                _currentPopup.IsOpen = false;
            }

            _currentPopup = null;
            _currentRequest = null;
            _isShowing = false;

            // اگر چیزی در صف هست، فقط همان «آخرین» را نمایش بده
            if (_pendingNotifications.Count > 0)
            {
                // چون همیشه فقط آخرین را نگه می‌داریم، همین dequeue کافی است
                ShowPopup(_pendingNotifications.Dequeue());
            }
        }

        private void UpdateCurrentVisual(NotificationRequest req)
        {
            if (req?.TextBox != null)
            {
                req.TextBox.Text = req.DisplayMessage;
            }
        }

        private void ResetTimer(int seconds)
        {
            _timer.Stop();
            _timer.Interval = TimeSpan.FromSeconds(Math.Max(1, seconds));
            _timer.Start();
        }

        private static int AddSecondsCapped(int baseSeconds, int add, int cap)
            => Math.Min(cap, baseSeconds + add);
    }
}
