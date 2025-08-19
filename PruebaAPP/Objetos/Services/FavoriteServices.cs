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
                    try
                    {
                        var json = File.ReadAllText(_filePath);
                        _favorite = JsonSerializer.Deserialize<List<Favorite>>(json) ?? [];

                    } catch{
                        Clear();
                    }
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

        // Retornar
            public Favorite? GetValue(string id)
            {
                return _favorite.FirstOrDefault(f => f.Id == id);
            }

        // Agregar favorito
            public bool Add(Favorite favorito)
            {
                if (!_favorite.Any(f => f.Id == favorito.Id))
                {
                    _favorite.Add(favorito);
                    Save();
                    return true;
                }
                return false;
            }

        // Eliminar favorito
            public void Delete(Favorite favorito)
            {
                var item = _favorite.FirstOrDefault(f => f.Id == favorito.Id);
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
                    => _favorite.Any(f => f.Id == favorito.Id);
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
