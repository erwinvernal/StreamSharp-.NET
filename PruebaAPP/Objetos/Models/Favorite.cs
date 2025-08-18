using CommunityToolkit.Mvvm.ComponentModel;

namespace PruebaAPP.Objetos.Models
{
    public partial class Favorite : ObservableObject
    {
        public string? Id         { get; set; }  // Id del recurso
        public string? Title      { get; set; }  // Nombre 
        public string? Author     { get; set; }  // Opcional
        public string? Thumbnails { get; set; }  // Opcional
        public string? Duracion   { get; set; }  // Opcional
        [ObservableProperty] public partial int IsPlay { get; set; }   // Estado de reproducción (0: Sin reproducir, 1: Reproduciendo, 2: Reproducido)
    }

}
