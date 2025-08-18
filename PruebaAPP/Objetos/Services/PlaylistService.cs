using PruebaAPP.Objetos.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace PruebaAPP.Objetos.Services
{
    public class PlaylistService
    {
        private readonly string _filePath;
        private List<Playlist> _playlists = [];
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public PlaylistService(string fileName = "playlists.json")
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            Load();
        }

        // Cargar JSON
        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _playlists = JsonSerializer.Deserialize<List<Playlist>>(json) ?? [];
            }
        }

        // Guardar JSON
        private void Save()
        {
            var json = JsonSerializer.Serialize(_playlists, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }

        // Obtener todas las playlists
        public List<Playlist> GetAll() => _playlists;

        // Crear nueva playlist
        public Playlist Create(string title)
        {
            var playlist = new Playlist { Id = _playlists.Count.ToString(), Title = title, CurrentDate = DateTime.Now };
            _playlists.Add(playlist);
            Save();
            return playlist;
        }

        // Eliminar playlist por Id
        public void Delete(string id)
        {
            var pl = _playlists.FirstOrDefault(p => p.Id == id);
            if (pl != null)
            {
                _playlists.Remove(pl);
                Save();
            }
        }

        // Elimina toda las playlist
        public void ClearAll()
        {
            _playlists.Clear();
            Save();
        }

        // Agregar canción a playlist
        public void AddFavorite(string playlistId, Favorite favorito)
        {
            var pl = _playlists.FirstOrDefault(p => p.Title == playlistId);
            if (pl != null && !pl.Items.Any(f => f.Title == favorito.Title))
            {
                pl.Items.Add(favorito);
                Save();
            }
        }

        // Quitar canción de playlist
        public void RemoveFavorite(string playlistId, string favoriteId)
        {
            var pl = _playlists.FirstOrDefault(p => p.Id == playlistId);
            if (pl != null)
            {
                var fav = pl.Items.FirstOrDefault(f => f.Id == favoriteId);
                if (fav != null)
                {
                    pl.Items.Remove(fav);
                    Save();
                }
            }
        }

        // Obtener canciones de una playlist
        public ObservableCollection<Favorite> GetFavorites(string playlistId)
        {
            return _playlists.FirstOrDefault(p => p.Id == playlistId)?.Items ?? [];
        }
    }
}
