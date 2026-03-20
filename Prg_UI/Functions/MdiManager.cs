using System;
using System.Windows;
using System.Windows.Controls;

namespace Prg_UI.Functions
{
    /// <summary>
    /// مدیریت پنجره‌های MDI - باز کردن پنجره‌ها داخل پنجره اصلی به جای پنجره جداگانه
    /// </summary>
    public static class MdiManager
    {
        /// <summary>
        /// کنواس اصلی که پنجره‌های MDI درون آن باز می‌شوند (از WinBase تنظیم می‌شود)
        /// </summary>
        public static Canvas? MdiCanvas { get; set; }

        /// <summary>
        /// آیا سیستم MDI فعال است
        /// </summary>
        public static bool IsEnabled => MdiCanvas != null;

        /// <summary>
        /// نمایش پنجره داخل محیط MDI
        /// </summary>
        public static void ShowInMdi(Window window)
        {
            if (MdiCanvas == null) return;

            var host = new MdiWindowHost();
            host.HostWindow(window);

            // اندازه‌گذاری: استفاده از SizeToContent ویندوز یا پیش‌فرض معقول
            double winWidth = double.IsNaN(window.Width) || window.Width <= 0 ? 900 : window.Width;
            double winHeight = double.IsNaN(window.Height) || window.Height <= 0 ? 600 : window.Height;

            // محدود کردن به اندازه canvas
            double canvasW = MdiCanvas.ActualWidth > 0 ? MdiCanvas.ActualWidth : 1200;
            double canvasH = MdiCanvas.ActualHeight > 0 ? MdiCanvas.ActualHeight : 700;

            winWidth = Math.Min(winWidth, canvasW - 20);
            winHeight = Math.Min(winHeight, canvasH - 20);

            host.Width = winWidth;
            host.Height = winHeight;

            // قرار دادن در مرکز با کمی offset تصادفی برای جلوگیری از قرار گرفتن روی هم
            var rnd = new Random();
            double left = Math.Max(0, (canvasW - winWidth) / 2 + rnd.Next(-40, 40));
            double top = Math.Max(0, (canvasH - winHeight) / 2 + rnd.Next(-30, 30));

            // اطمینان از اینکه داخل canvas باشد
            left = Math.Min(left, canvasW - winWidth);
            top = Math.Min(top, canvasH - 36);

            Canvas.SetLeft(host, Math.Max(0, left));
            Canvas.SetTop(host, Math.Max(0, top));
            Canvas.SetZIndex(host, MdiCanvas.Children.Count + 1);

            // وقتی پنجره بسته شد، host را از canvas حذف کن
            host.CloseRequested += (s, e) =>
            {
                MdiCanvas.Children.Remove(host);
            };

            MdiCanvas.Children.Add(host);
            host.BringToFront();

            // اطمینان از اینکه ویندوز اصلی در نوار وظیفه نشان داده می‌شود
            // window را نشان نمی‌دهیم - فقط محتوایش را در MDI نشان دادیم
        }

        /// <summary>
        /// آوردن پنجره MDI به جلو
        /// </summary>
        public static void BringWindowToFront(Window window)
        {
            if (MdiCanvas == null) return;

            foreach (UIElement child in MdiCanvas.Children)
            {
                if (child is MdiWindowHost host && host.IsHosting(window))
                {
                    host.BringToFront();
                    return;
                }
            }
        }

        /// <summary>
        /// بستن تمام پنجره‌های MDI
        /// </summary>
        public static void CloseAll()
        {
            if (MdiCanvas == null) return;

            for (int i = MdiCanvas.Children.Count - 1; i >= 0; i--)
            {
                if (MdiCanvas.Children[i] is MdiWindowHost host)
                {
                    host.CloseHost();
                }
            }
        }
    }
}
