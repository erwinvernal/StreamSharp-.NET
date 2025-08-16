using PruebaAPP.Objetos.Models;
using System.Text.Json;

namespace PruebaAPP.Objetos.Services
{
    public class FavoriteServices
    {
        // Variables del servicio
            private readonly string _filePath;
            private List<Favorite> _favorite = [];

        // Inicializacion del contructor
            public FavoriteServices(string fileName = "favoritos.json")
            {
                _filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                Load();
            }

        // Cargar desde JSON
            private void Load()
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    _favorite = JsonSerializer.Deserialize<List<Favorite>>(json) ?? [];
                }
                else
                {
                    _favorite = new List<Favorite>();
                }
            }

        // Guardar JSON
            private void Save()
            {
                var json = JsonSerializer.Serialize(_favorite, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }

        // Obtener todos los favoritos
            public List<Favorite> GetAll() => _favorite;

        // Obtener por Type
            public List<Favorite> GetByType(FType Type)
                => _favorite.Where(f => f.Type == Type).ToList();

        // Retornar
            public Favorite? GetValue(string id)
            {
                return _favorite.FirstOrDefault(f => f.Id == id);
            }

        // Agregar favorito
            public void Add(Favorite favorito)
            {
                if (!_favorite.Any(f => f.Id == favorito.Id && f.Type == favorito.Type))
                {
                    _favorite.Add(favorito);
                    Save();
                }
            }

        // Eliminar favorito
            public void Delete(Favorite favorito)
            {
                var item = _favorite.FirstOrDefault(f => f.Id == favorito.Id && f.Type == favorito.Type);
                if (item != null)
                {
                    _favorite.Remove(item);
                    Save();
                }
            }
            public void Delete(string id)
            {
                var item = _favorite.FirstOrDefault(f => f.Id == id);
                if (item != null)
                {
                    _favorite.Remove(item);
                    Save();
                }
            }

        // Verificar si existe
            public bool Exists(Favorite favorito)
                    => _favorite.Any(f => f.Id == favorito.Id && f.Type == favorito.Type);
            public bool Exists(string id)
            {
                return _favorite.Any(f => f.Id == id);
            }

        // Limpiar todos (opcional)
            public void Clear()
                {
                    _favorite.Clear();
                    Save();
                }
    }
}
