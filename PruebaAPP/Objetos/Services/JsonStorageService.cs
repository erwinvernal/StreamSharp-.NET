using PruebaAPP.Objetos.Enum;
using System.Text.Json;

namespace PruebaAPP.Objetos.Services
{
    public class JsonStorageService<T> where T : class
    {
        private readonly string _filePath;
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        public JsonStorageService(StorageType type)
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, GetFileName(type));

            // Crear archivo vacío si no existe
            if (!File.Exists(_filePath))
            {
                Save([]);
            }
        }

        private List<T> Load()
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<T>>(json) ?? [];
        }


        private void Save(List<T> items)
        {
            var json = JsonSerializer.Serialize(items, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }

        public List<T> GetAll() => Load();

        public void Add(T item)
        {
            var items = Load();
            items.Add(item);
            Save(items);
        }

        public void Update(Func<T, bool> predicate, Action<T> updateAction)
        {
            var items = Load();
            var item = items.FirstOrDefault(predicate);
            if (item != null)
            {
                updateAction(item);
                Save(items);
            }
        }

        public void Delete(Func<T, bool> predicate)
        {
            var items = Load();
            var item = items.FirstOrDefault(predicate);
            if (item != null)
            {
                items.Remove(item);
                Save(items);
            }
        }

        public void Clear()
        {
            Save([]);
        }

        public bool Exists(Func<T, bool> predicate)
        {
            var items = Load();
            return items.Any(predicate);
        }

        // Funciones del enum
        private static string GetFileName(StorageType type)
        {
            return type switch
            {
                StorageType.Favorites => "favorites.json",
                StorageType.Playlists => "playlists.json",
                StorageType.PlaybackHistory => "playbackHistory.json",
                _ => throw new NotImplementedException()
            };
        }
    }
}
