using System;
using System.Globalization;
using System.Text.RegularExpressions;
namespace PruebaAPP.Converters
{
    public class TextNormalizerConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                // 1️ Quitar espacios extras al inicio y final
                text = text.Trim();

                // 2️ Reemplazar múltiples espacios por uno solo
                text = Regex.Replace(text, @"\s+", " ");

                // 3️ Quitar símbolos raros / caracteres no deseados
                // Podés personalizar la lista según necesites
                text = Regex.Replace(text, @"[^\w\sáéíóúÁÉÍÓÚñÑ]", "");

                // 4️ Capitalizar la primera letra de cada palabra
                text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());

                return text;
            }

            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
