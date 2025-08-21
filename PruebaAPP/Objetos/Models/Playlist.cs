using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PruebaAPP.Objetos.Models
{
    public class Playlist
    {
        // Propiedades
        public string? Id { get; set; } = String.Empty;              // Identificador único
        public string? Title { get; set; } = String.Empty;           // Nombre de la playlist
        public ObservableCollection<Song> Items { get; set; } = [];  // Lista de canciones
        public bool IsPlaying { get; set; } = false;                 // Indica si la playlist está en reproducción

        // Funciones auxiliares
        [JsonIgnore] public TimeSpan GetTotalDuration => Items.Aggregate(TimeSpan.Zero, (total, song) => total + song.Duration.GetValueOrDefault());
    }
}
