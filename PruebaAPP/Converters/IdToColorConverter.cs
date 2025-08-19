using PruebaAPP.Objetos.Models;
using PruebaAPP.Views.Android.ViewModels;
using System.Globalization;

namespace PruebaAPP.Converters
{
    public class IdToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Verifica que parameter no sea null y que sea de tipo Binding antes de hacer el cast
            if (parameter is Binding a)
            {
                // Verifica si el BindingContext es un BindableObject y si es del tipo AndroidPropertyViewModel
                if (a.Source is BindableObject bindable && bindable.BindingContext is AndroidPropertyViewModel vm)
                {


                    return value?.ToString() == vm.CurrentSong?.Id
                        ? Colors.Red      // color activo
                        : Colors.Black;   // color normal
                }
            }
            return Colors.Black;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
