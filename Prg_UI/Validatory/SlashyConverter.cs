using System;
using System.Globalization;
using System.Windows.Data;

namespace Validatory
{
    public class SlashyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? input = value?.ToString();
            // Check if the value is null or an empty string
            if (value == null || input.Length != 8 || string.IsNullOrWhiteSpace(value?.ToString()) || string.IsNullOrEmpty(value?.ToString()))
            {
                //return string.Empty;
                return null;
            }
            else
            {
                return $"{input.Substring(0, 4)}/{input.Substring(4, 2)}/{input.Substring(6, 2)}";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Implement conversion back if necessary, or throw an exception if not supported
            throw new NotImplementedException();
        }
    }
}
