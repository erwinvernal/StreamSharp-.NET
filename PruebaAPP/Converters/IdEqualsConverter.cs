using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaAPP.Converters
{
    public class IdEqualsConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string id && parameter is string selectedId)
                return id == selectedId;
            return false;
        }

        public object ConvertBack(object?  value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
