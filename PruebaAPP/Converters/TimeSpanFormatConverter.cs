using System.Globalization;

namespace PruebaAPP.Converters
{
    public class TimeSpanFormatConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            TimeSpan ts;

            if (value is string str)
            {
                // Intentamos parsear la cadena a TimeSpan
                if (!TimeSpan.TryParse(str, out ts))
                    ts = TimeSpan.Zero; // valor por defecto si falla
            }
            else if (value is TimeSpan t)
            {
                ts = t;
            }
            else
            {
                ts = TimeSpan.Zero;
            }

            // Formateamos según duración
            if (ts.TotalHours >= 1)
                return ts.ToString(@"hh\:mm\:ss");
            else
                return ts.ToString(@"mm\:ss");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (TimeSpan.TryParse(value?.ToString(), out var ts))
                return ts;

            return TimeSpan.Zero;
        }
    }
}
