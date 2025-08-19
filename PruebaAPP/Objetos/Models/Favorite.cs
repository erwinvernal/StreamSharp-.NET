using CommunityToolkit.Mvvm.ComponentModel;
using PruebaAPP.Views.Android.ViewModels;
using System.ComponentModel;

namespace PruebaAPP.Objetos.Models
{
    public partial class Favorite : ObservableObject
    {
        [ObservableProperty] public partial string? Id         { get; set; }  // Id del recurso
        [ObservableProperty] public partial string? Title      { get; set; }  // Nombre 
        [ObservableProperty] public partial string? Author     { get; set; }  // Opcional
        [ObservableProperty] public partial string? Thumbnails { get; set; }  // Opcional
        [ObservableProperty] public partial string? Duracion   { get; set; }  // Opcional
        [ObservableProperty] public partial bool    IsPlay     { get; set; }  // Estado de reproducción (0: Sin reproducir, 1: Reproduciendo, 2: Reproducido)

    }

}
