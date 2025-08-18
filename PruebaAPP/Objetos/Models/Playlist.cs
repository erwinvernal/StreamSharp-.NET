using System.Collections.ObjectModel;

namespace PruebaAPP.Objetos.Models
{
    public class Playlist
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Identificador único
        public string? Title { get; set; } = null;           // Nombre de la playlist
        public DateTime? CurrentDate { get; set; } = null;           // Nombre de la playlist
        public ObservableCollection<Favorite> Items { get; set; } = [];          // Lista de canciones

    }
}
