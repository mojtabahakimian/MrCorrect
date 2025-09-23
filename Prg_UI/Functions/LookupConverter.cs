using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Prg_UI.Functions
{
    public class LookupConverter : IValueConverter
    {
        public IDictionary<string, string> Map { get; set; }
        private static string Norm(object o) => CL_LMethods.NormalizeArabicPersian(System.Convert.ToString(o) ?? string.Empty).Trim();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || Map == null) return null;
            return Map.TryGetValue(Norm(value), out var text) ? text : value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
