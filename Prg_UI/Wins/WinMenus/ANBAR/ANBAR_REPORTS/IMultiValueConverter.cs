using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace Prg_UI.Wins.WinMenus.ANBAR.ANBAR_REPORTS
{
    public class DynamicDecimalFormatter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] => عدد اصلی (MAND)
            // values[1] => تعداد ارقام اعشار (DIG)

            // بررسی می‌کنیم که ورودی‌ها معتبر باشند
            if (values.Length < 2 || values[0] == null || values[1] == null ||
                !(values[0] is IConvertible) || !(values[1] is IConvertible))
            {
                return "0"; // یا هر مقدار پیش‌فرض دیگر
            }

            try
            {
                decimal number = System.Convert.ToDecimal(values[0]);
                int decimalPlaces = System.Convert.ToInt32(values[1]);

                // ساخت رشته فرمت به صورت دینامیک
                // مثلا اگر decimalPlaces = 3 باشد، فرمت می‌شود "#,##0.000"
                string format;
                if (decimalPlaces > 0)
                {
                    format = $"#,##0.{new string('0', decimalPlaces)}";
                }
                else
                {
                    format = "#,##0";
                }

                return number.ToString(format);
            }
            catch (Exception)
            {
                // اگر تبدیل ناموفق بود، مقدار اصلی را برگردان
                return values[0];
            }
        }

        // ConvertBack برای این سناریو استفاده نمی‌شود
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
