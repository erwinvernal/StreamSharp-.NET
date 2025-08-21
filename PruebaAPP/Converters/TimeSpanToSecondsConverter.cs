using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaAPP.Converters
{
    public class TimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
                return ts.TotalSeconds;
            return 0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double seconds)
                return TimeSpan.FromSeconds(seconds);
            return TimeSpan.Zero;
        }
    }
}
