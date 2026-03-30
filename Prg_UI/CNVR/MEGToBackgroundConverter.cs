using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Prg_UI.CNVR
{
    public class MegBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // بررسی نال نبودن و تبدیل ایمن مقدار به عدد اعشاری (پشتیبانی از decimal/double/int)
            if (value != null && double.TryParse(value.ToString(), out double megValue))
            {
                if (megValue > 0)
                {
                    // سبز روشن
                    return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#CCFFCC"));
                }
                else if (megValue < 0)
                {
                    // قرمز روشن
                    return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDDDD"));
                }
            }

            // رنگ پیش‌فرض (شفاف) در صورتی که مقدار صفر باشد یا دیتایی وجود نداشته باشد
            return System.Drawing.Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // تبدیل معکوس در این سناریو نیازی نیست
            throw new NotSupportedException();
        }
    }
}
