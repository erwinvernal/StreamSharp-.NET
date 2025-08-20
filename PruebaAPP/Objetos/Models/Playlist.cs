using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace PruebaAPP.Objetos.Models
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class Playlist
    {
        public string? Id { get; set; } = String.Empty;              // Identificador único
        public string? Title { get; set; } = String.Empty;           // Nombre de la playlist
        public ObservableCollection<Song> Items { get; set; } = [];  // Lista de canciones

    }
}
