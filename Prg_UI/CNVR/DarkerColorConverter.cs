using System;
using System.Globalization;
using System.Windows.Data;

namespace Prg_UI.CNVR
{
    public class DarkerColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte colorValue)
            {
                // Darken by reducing brightness by 30%
                return (byte)Math.Max(0, colorValue * 0.7);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /*
     *      xmlns:VERT="clr-namespace:Prg_UI.CNVR"

    <Window.Resources>

        <VERT:DarkerColorConverter x:Key="DarkerColorConverter"/>
     */

}
