using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Prg_UI.Wins.WinMenus.BARNAME_RIZI
{
    public class ThousandsSeparatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            if (double.TryParse(value.ToString(), out double d))
                return d.ToString("#,##0", CultureInfo.InvariantCulture);
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

        private string ToFaNumber(string str)
        {
            return str.Replace('0', '۰')
                      .Replace('1', '۱')
                      .Replace('2', '۲')
                      .Replace('3', '۳')
                      .Replace('4', '۴')
                      .Replace('5', '۵')
                      .Replace('6', '۶')
                      .Replace('7', '۷')
                      .Replace('8', '۸')
                      .Replace('9', '۹');
        }
    }
}
