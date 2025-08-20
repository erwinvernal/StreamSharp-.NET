using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PruebaAPP.Converters
{
    public class GreaterThanConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (double.TryParse(value.ToString(), out double input) &&
                double.TryParse(parameter.ToString(), out double compareValue))
            {
                return input >= compareValue;
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
