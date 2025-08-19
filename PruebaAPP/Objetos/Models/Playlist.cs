using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace PruebaAPP.Objetos.Models
{
    public partial class Playlist : ObservableObject
    {
        [ObservableProperty] public partial string Id { get; set; }  // Identificador único
        [ObservableProperty] public partial string? Title { get; set; } = null;           // Nombre de la playlist
        [ObservableProperty] public partial DateTime? CurrentDate { get; set; } = null;           // Nombre de la playlist
        [ObservableProperty] public partial ObservableCollection<Song> Items { get; set; } = [];          // Lista de canciones

        // Propiedad calculada
        public TimeSpan TotalDuration => Items.Aggregate(TimeSpan.Zero, (total, song) => total + song.Duration.GetValueOrDefault());
    }
}
