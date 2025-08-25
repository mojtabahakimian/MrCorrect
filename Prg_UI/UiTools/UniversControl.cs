using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Prg_UI.UiTools
{
    public class UniversControl
    {
        System.Windows.Threading.DispatcherTimer MyTimer = new System.Windows.Threading.DispatcherTimer();
        Popup My_Pouop = null;
        private readonly Queue<NotificationRequest> _pendingNotifications = new();
        private static readonly BrushConverter _brushConverter = new();
        private const int MaxQueueSize = 100;
        private bool _isShowing;
        private sealed class NotificationRequest
        {
            public string Message { get; }
            public Popup Popup { get; }
            public TextBox TextBox { get; }
            public Border Border { get; }
            public string Background { get; }
            public int WaitSeconds { get; }
            public NotificationRequest(string message, Popup popup, TextBox textBox, Border border, string background, int waitSeconds)
            {
                Message = message;
                Popup = popup;
                TextBox = textBox;
                Border = border;
                Background = background;
                WaitSeconds = waitSeconds;
            }
        }
        public enum RangPop
        {
            Red,
            Green,
            Blue,
            Yellow
        }

        /// <summary>
        /// Blue  : #FF007ACC
        ///
        /// Red   
        ///
        /// Green : #FF1AAA2C
        ///
        /// Yellow: #FFDC9E18
        /// </summary>
        /// <param name="Msgtext"></param>
        /// <param name="Secound_Wait"></param>
        /// <param name="Rang_Back"></param>
        /// <param name="StayShowinger">با کلیک روی یک قسمتی پاپ آپ مخفی شود یا همینطور باقی بماند , بدون تایمر اگر تنظیم شده باشد</param>
        public void PopNotifyShow(string Msgtext, Popup _thePopup = null, TextBox textOfPop = null, Border bordOfPop = null, string Rang_Back = "#E5EC2B2B", int Secound_Wait = 2)
        {
            var request = new NotificationRequest(Msgtext, _thePopup, textOfPop, bordOfPop, Rang_Back, Secound_Wait);
            if (_isShowing)
            {
                if (_pendingNotifications.Count >= MaxQueueSize)
                {
                    _pendingNotifications.Dequeue();
                }
                _pendingNotifications.Enqueue(request);
            }
            else
            {
                ShowPopup(request);
            }
        }
        public void PopNotifyShowUp(string Msgtext, Popup _thePopup = null, TextBox textOfPop = null, Border bordOfPop = null, RangPop rangPop = RangPop.Red, int Secound_Wait = 2)
        {
            string RangStr = "";
            switch (rangPop)
            {
                case RangPop.Blue: RangStr = "#FF007ACC"; break;
                case RangPop.Red: RangStr = "#E5EC2B2B"; break;
                case RangPop.Green: RangStr = "#FF1AAA2C"; break;
                case RangPop.Yellow: RangStr = "#FFDC9E18"; break;
                default: RangStr = "#E5EC2B2B"; break;
            }
            PopNotifyShow(Msgtext, _thePopup, textOfPop, bordOfPop, RangStr, Secound_Wait);
        }
        private void ShowPopup(NotificationRequest request)
        {
            _isShowing = true;
            My_Pouop = request.Popup;
            var color = string.IsNullOrEmpty(request.Background) ? "#E5EC2B2B" : request.Background;
            request.Border.Background = (Brush)_brushConverter.ConvertFrom(color);
            request.TextBox.Text = request.Message;
            My_Pouop.IsOpen = true;

            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Interval = TimeSpan.FromSeconds(request.WaitSeconds);
            MyTimer.Start();
        }
        public void MyTimer_Tick(object sender, EventArgs e)
        {
            MyTimer.Stop();
            MyTimer.IsEnabled = false;
            MyTimer.Tick -= MyTimer_Tick;
            My_Pouop.IsOpen = false;
            My_Pouop = null;
            _isShowing = false;
            if (_pendingNotifications.Count > 0)
            {
                ShowPopup(_pendingNotifications.Dequeue());
            }
        }
    }
}