using System.Globalization;

namespace PruebaAPP.Converters
{
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IConvertible convertible)
            {
                double number = convertible.ToDouble(culture);

                // Si no se pasa parámetro, por defecto usamos 0
                double threshold = 0;
                if (parameter is not null && double.TryParse(parameter.ToString(), out double parsed))
                    threshold = parsed;

                return number == threshold;
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                double threshold = 0;
                if (parameter is not null && double.TryParse(parameter.ToString(), out double parsed))
                    threshold = parsed;

                return boolValue ? threshold : threshold + 1;
            }

            return 0;
        }
    }
}
