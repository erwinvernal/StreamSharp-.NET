using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PruebaAPP.Objetos.Functions;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Objetos.Services;
using PruebaAPP.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;
using PruebaAPP.Objetos.Enum;

namespace PruebaAPP.Views.Android.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // =============================================================================================
        // == Inicializacion del contructor
        // =============================================================================================
        public MainViewModel(PlayerViewModel mediaService, IDispatcher dispatcher)
        {
            // Inicializacion de los servicios
            _youtubeService  = new YouTubeService();
            _favoriteService = new JsonStorageService<Song>(StorageType.Favorites);
            _playlistService = new JsonStorageService<Playlist>(StorageType.Playlists);
            _playbackService = new JsonStorageService<PlaybackHistory>(StorageType.PlaybackHistory);
            _dispatcher = dispatcher;

            // Asignamos el reproductor de música
            Player = mediaService;
            Player.ViewM = this;

            // Cargamos datos iniciales
            Favoritos       = new ObservableCollection<Song>(_favoriteService.GetAll());
            Playlists       = new ObservableCollection<Playlist>(_playlistService.GetAll());
            PlayBackHistory = new ObservableCollection<PlaybackHistory>(_playbackService.GetAll());

            // Precarga por defecto
            var textrandom = GetRandomMusicText();
            _ = Search(textrandom);
        }

        // =============================================================================================
        // == Declaracion de servicios
        // =============================================================================================
        private readonly YouTubeService _youtubeService;                                // Servicio de YouTube
        private readonly JsonStorageService<Song> _favoriteService;                     // Servicio de favoritos
        private readonly JsonStorageService<Playlist> _playlistService;                 // Servicio de playlists
        private readonly JsonStorageService<PlaybackHistory> _playbackService;          // Servicio de historial de reproducción
        private readonly IDispatcher _dispatcher;                                       // Dispatcher para operaciones asincrónicas

        // =============================================================================================
        // == Propiedades de la vista
        // =============================================================================================
        public PlayerViewModel Player { get; }                                          // Reproductor de música

        // =============================================================================================
        // == Listas de datos
        // =============================================================================================
        public ObservableCollection<Song> Favoritos { get; }                            // Lista de favoritos
        public ObservableCollection<Playlist> Playlists { get; }                        // Lista de playlists
        public ObservableCollection<PlaybackHistory> PlayBackHistory { get; }           // Lista de historial de reproducción
        public ObservableCollection<VideoSearchResult> ListItems { get; set; } = [];    // Lista de resultados de búsqueda

        // =============================================================================================
        // == Otras propiedades
        // =============================================================================================
        [ObservableProperty] public partial View? CurrentView { get; set; }             // Vista actual
        [ObservableProperty] public partial bool IsLoading { get; set; }                // Indicador de carga
        [ObservableProperty] public partial bool IsLoading2 { get; set; }               // Indicador de carga para operaciones secundarias

        // =============================================================================================
        // == Variables de control publicas
        // =============================================================================================
        public bool IsBlockClick { get; set; } = false;                                 // Bloqueo de clics múltiples
        public record PlaylistRemoveParam(string PlaylistId, string FavoriteId);        // Parámetros para eliminar canción de playlist

        // =============================================================================================
        // == Variables de control privadas
        // =============================================================================================
        private bool _isPlayingSong = false;                                            // Indicador de reproducción en curso
        private readonly List<VideoSearchResult> _cache = [];                           // Caché de resultados de búsqueda
        private string _currentSearch = string.Empty;                                   // Texto de búsqueda actual
        private IAsyncEnumerator<VideoSearchResult>? _searchEnumerator;                 // Enumerador de resultados de búsqueda
        private bool _isLoadingMore;                                                    // Indicador de carga adicional
        private bool _isSearching;                                                      // Indicador de búsqueda en curso
        private int _loadedCount = 0;                                                   // Contador de elementos cargados desde la caché

        // =============================================================================================
        // == Busqueda y carga de resultados
        // =============================================================================================
        public async Task Search(string query)
        {
            if (_isSearching || string.IsNullOrWhiteSpace(query))
                return;

            _isSearching = true;
            IsLoading = true;

            try
            {
                await RetryAsync(async () =>
                {
                    _currentSearch = query.Trim();
                    _loadedCount = 0;
                    _cache.Clear();
                    ListItems.Clear();

                    _searchEnumerator = _youtubeService.GetSearchEnumerator(_currentSearch);

                    await LoadMore(10);
                });
            }
            finally
            {
                _isSearching = false;
                IsLoading = false;
            }
        }
        public async Task LoadMore(int count = 10)
        {
            if (_isLoadingMore || _searchEnumerator == null)
                return;

            _isLoadingMore = true;

            try
            {
                int loaded = 0;

                // Consumir cache primero
                var newItems = new List<VideoSearchResult>();
                while (loaded < count && _loadedCount < _cache.Count)
                {
                    var item = _cache[_loadedCount];
                    if (!ListItems.Contains(item))
                        newItems.Add(item);

                    loaded++;
                    _loadedCount++;
                }

                if (newItems.Count > 0)
                {
                    await _dispatcher.DispatchAsync(() =>
                    {
                        foreach (var i in newItems)
                            ListItems.Add(i);
                    });
                }

                // Consumir enumerador (con RetryAsync)
                while (loaded < count)
                {
                    var hasNext = await RetryAsync(async () =>
                    {
                        return await _searchEnumerator.MoveNextAsync();
                    });

                    if (!hasNext) break;

                    var item = _searchEnumerator.Current;

                    // Filtro: duración mínima 1 minuto
                    if (item.Duration != null &&
                        item.Duration.Value.TotalMinutes > 1 &&
                        !_cache.Contains(item))
                    {
                        _cache.Add(item);
                        await _dispatcher.DispatchAsync(() =>
                        {
                            ListItems.Add(item);
                        });
                        loaded++;
                    }
                }
            }
            finally
            {
                _isLoadingMore = false;
            }
        }
        private static async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxRetries = 3, int delayMs = 1000)
        {
            int retries = 0;
            while (true)
            {
                try
                {
                    return await action();
                }
                catch
                {
                    retries++;
                    if (retries >= maxRetries)
                        throw;

                    await Task.Delay(delayMs);
                }
            }
        }
        private static async Task RetryAsync(Func<Task> action, int maxRetries = 3, int delayMs = 1000)
        {
            await RetryAsync(async () =>
            {
                await action();
                return true; // devuelve algo simple para satisfacer <T>
            }, maxRetries, delayMs);
        }

        // =============================================================================================
        // == Reproducción de canciones
        // =============================================================================================
        public async Task Play(string id)
        {
            // Validamos
            if (string.IsNullOrWhiteSpace(id))
                return;

            // Validamos estado
            _isPlayingSong = true;
            IsLoading2 = true;
            Player.CurrentSong = null;

            // 
            UpdateSong(id);

            // Empezamos proceso
            try
            {
                // Detener la canción actual
                Player.Controller.Stop();

                // Obtener detalles del video
                var item = await SafeGetVideoDetailsAsync(id);
                if (item == null)
                    return;

                // Obtener stream de audio
                var streamInfo = await SafeGetAudioStreamAsync(item.Url);
                if (streamInfo == null)
                    return;

                // Configurar metadata
                Player.Controller.MetadataTitle = item.Title;
                Player.Controller.MetadataArtist = item.Author.ChannelTitle;
                Player.Controller.MetadataArtworkUrl = ThumbnailHelper.GetHighestThumbnail(item.Thumbnails);

                // Crear y asignar canción
                Player.CurrentSong = CreateSong(item);

                // Reproducir
                Media_Load(streamInfo.Url);
            }
            finally
            {
                IsLoading2 = false;
                _isPlayingSong = false;
            }
        }
        private async Task<VideoSearchResult?> SafeGetVideoDetailsAsync(string id)
        {
            try
            {
                return await _youtubeService.GetVideoDetailsAsync(id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener detalles: {ex.Message}");
                await ShowError("No se pudo obtener la canción.");
                return null;
            }
        }
        private async Task<IStreamInfo?> SafeGetAudioStreamAsync(string url)
        {
            try
            {
                var stream = await Task.Run(() => _youtubeService.GetBestAudioStreamAsync(url));
                if (stream == null)
                    await ShowError("No se pudo obtener el audio.");
                return stream;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al obtener stream: {ex.Message}");
                await ShowError("No se pudo obtener el audio.");
                return null;
            }
        }
        private static async Task ShowError(string message)
        {
            await DialogHelpers.DisplayMessage("Error", message, "Aceptar");
        }
        private Song CreateSong(VideoSearchResult item)
        {
            return new()
            {
                Id = item.Id,
                Title = item.Title,
                Author = new AuthorSong
                {
                    ChannelId = item.Author.ChannelId,
                    ChannelTitle = item.Author.ChannelTitle,
                    ChannelUrl = item.Author.ChannelUrl
                },
                ThumbnailHighRes = ThumbnailHelper.GetHighestThumbnail(item.Thumbnails),
                ThumbnailLowRes = ThumbnailHelper.GetLowestThumbnail(item.Thumbnails),
                Duration = item.Duration,
                IsFavorite = _favoriteService.Exists(s => s.Id! == item.Id)
            };
        }
        public void UpdateSong(string id)
        {
            // Actualizar canciones en favoritos
            foreach (var favorite in Favoritos)
                favorite.IsPlay = favorite.Id == id;

            // Actualizar canciones en playlists
            foreach (var playlist in Playlists)
            {
                foreach (var song in playlist.Items)
                    song.IsPlay = song.Id == id;
            }

        }


        // =============================================================================================
        // == Comandos de favoritos
        // =============================================================================================
        [RelayCommand] public void Favorite_Add(Song favorito)
        {
            // Agregamos a la lista observable del ViewModel
            _favoriteService.Add(favorito);
            Favoritos.Add(favorito);

            // Comprobamos si esta sonando
            if (favorito.Id == Player.CurrentSong?.Id)
                Player.CurrentSong?.IsFavorite = true;

            // Mostramos si esta sonando
            UpdateSong(favorito.Id!);

            // Notificamos cambios
            OnPropertyChanged(nameof(Favorite_GetTotalD));

        }
        [RelayCommand] public async Task Favorite_Delete(string id)
        {
            // Pedimos confirmacion
            var title   = "¿Quitar de favoritos?";
            var message = "Esta acción no se puede deshacer. ¿Estás seguro de continuar?";
            var ok      = "Eliminar";
            var cancel  = "Atrás";
            var result  = await DialogHelpers.DisplayMessage(title, message, ok, cancel);
            if (!result) return;

            // Eliminar del servicio
            _favoriteService.Delete(s => s.Id == id);

            // Eliminar de la lista observable del ViewModel
            var item = Favoritos.FirstOrDefault(f => f.Id == id);
            if (item != null) Favoritos.Remove(item);

            // Verificamos si la cancion esta corriendo
            if (Player.CurrentSong != null && !string.IsNullOrEmpty(Player.CurrentSong.Id) && item != null && !string.IsNullOrEmpty(item.Id) && Player.CurrentSong.Id == item.Id)
            {
                Player.CurrentSong.IsFavorite = false;
            }

            // Notificamos cambios
            OnPropertyChanged(nameof(Favorite_GetTotalD));

        }
        [RelayCommand] public void Favorite_PlayAll()
        {

            // Creamos una playlist temporal
            Playlist playlist = new()
            {
                Id = "0",
                Title = "Mis lista de favoritos",
                Items = Favoritos
            };

            // Establecemos playlist
            Player.SelectedPlaylist = playlist;

            // Abrimos ventana
            CurrentView = new Android_View_Playlist();

        }

        public TimeSpan Favorite_GetTotalD => Favoritos.Aggregate(TimeSpan.Zero, (total, song) => total + song.Duration.GetValueOrDefault());

        // =============================================================================================
        // == Comandos de playlist
        // =============================================================================================
        [RelayCommand] public async Task Playlist_Created()
        {
            // Creamos entrada de inputbox
            string nombre = await DialogHelpers.DisplayPrompt(
                "Nueva Playlist",           // Título
                "Ingresa el nombre:",       // Mensaje
                "Crear",                    // Texto del botón aceptar
                "Cancelar",                 // Texto del botón cancelar
                "Nombre de la playlist",    // Placeholder
                50,                         // Límite de caracteres
                keyboard: Keyboard.Text     // Tipo de teclado
            );

            // Verificamos que el usuario haya ingresado algo
            if (string.IsNullOrWhiteSpace(nombre)) return;

            // Creamos la nueva playlist
            var playlist = new Playlist
            {
                Id = Guid.NewGuid().ToString(),  // Id único
                Title = nombre,
                Items = [],
                IsPlaying = false
            };

            // Guardamos en el servicio JSON
            _playlistService.Add(playlist);

            // Si tenés una colección en la UI para mostrar
            Playlists.Add(playlist);
        }
        [RelayCommand] public async Task Playlist_ToAdd(Song fav)
        {
            if (Playlists == null || Playlists.Count == 0) return;

            // Preparamos array de títulos desde la colección vinculada a la UI
            var playlistTitlesArray = Playlists.Select(p => p.Title!).ToArray();

            // Abrimos cuadro de diálogo
            var result = await DialogHelpers.DisplayAction("Selecciona playlist", "Cancelar", null, playlistTitlesArray);
            if (result == "Cancelar") return;

            // Buscamos la playlist directamente en la ObservableCollection
            var selectedPlaylist = Playlists.FirstOrDefault(p => p.Title == result);
            if (selectedPlaylist == null) return;

            // Aseguramos que la canción tenga Id
            if (string.IsNullOrEmpty(fav.Id))
                fav.Id = Guid.NewGuid().ToString();

            // Verificamos que la canción no exista ya en la playlist
            if (!selectedPlaylist.Items.Any(s => s.Id == fav.Id))
            {
                // Agregamos la canción a la colección observable
                selectedPlaylist.Items.Add(fav);

                // Guardamos cambios en JSON
                _playlistService.Update(p => p.Id == selectedPlaylist.Id, p =>
                {
                    p.Items = selectedPlaylist.Items;
                });
            }

            // Notificamos cambios en la UI
            OnPropertyChanged(nameof(Playlist_GetTotalSong));
            OnPropertyChanged(nameof(Playlists_GetTotalDuration));

        }
        [RelayCommand] public async Task Playlist_Delete(string playlistId)
        {
            // Pedimos confirmación
            var title = "¿Eliminar esta playlist?";
            var message = "Esta acción no se puede deshacer. ¿Estás seguro de continuar?";
            var ok = "Eliminar";
            var cancel = "Atrás";

            var result = await DialogHelpers.DisplayMessage(title, message, ok, cancel);
            if (!result) return;

            // Eliminar del servicio JSON
            _playlistService.Delete(p => p.Id == playlistId);

            // Eliminar de la lista observable del ViewModel
            var item = Playlists.FirstOrDefault(p => p.Id == playlistId);
            if (item != null)
                Playlists.Remove(item);

            // Notificamos cambios en la UI
            OnPropertyChanged(nameof(Playlist_GetTotalSong));
            OnPropertyChanged(nameof(Playlists_GetTotalDuration));
        }
        [RelayCommand] public async Task Playlist_ToDelete(PlaylistRemoveParam param)
        {
            // Verificamos si hay parámetro
            if (param == null) return;

            // Pedimos confirmación
            string title = "¿Quitar de la playlist?";
            string message = "No podrás revertir esta acción. ¿Deseas continuar?";
            string accept = "Quitar";
            string cancel = "Cancelar";

            bool result = await DialogHelpers.DisplayMessage(title, message, accept, cancel);
            if (!result) return;

            // Buscamos la playlist
            var pl = Playlists.FirstOrDefault(p => p.Id == param.PlaylistId);
            if (pl == null) return;

            // Buscamos la canción a eliminar
            var song = pl.Items.FirstOrDefault(s => s.Id == param.FavoriteId);
            if (song == null) return;

            // Eliminamos la canción del ObservableCollection
            pl.Items.Remove(song);

            // Guardamos cambios en el JSON
            _playlistService.Update(p => p.Id == pl.Id, p =>
            {
                p.Items = pl.Items; // Actualizamos la lista de canciones
            });

            // Notificamos cambios en la UI
            OnPropertyChanged(nameof(Playlist_GetTotalSong));
            OnPropertyChanged(nameof(Playlists_GetTotalDuration));
        }
        [RelayCommand] public async Task Playlist_ClearAll()
        {
            // Verificamos si hay playlists
            if (Playlists.Count == 0) return;

            // Pedimos confirmacion
            var title   = "¿Eliminar todas las playlist?";
            var message = "Esta acción no se puede deshacer. ¿Estás seguro de continuar?";
            var ok      = "Eliminar";
            var cancel  = "Atrás";
            var result  = await DialogHelpers.DisplayMessage(title, message, ok, cancel);
            if (!result) return;

            // Eliminamos
            _playlistService.Clear();
            Playlists.Clear();

            //Notificamos cambio
            OnPropertyChanged(nameof(Playlists));
            OnPropertyChanged(nameof(Playlist_GetTotalSong));
            OnPropertyChanged(nameof(Playlists_GetTotalDuration));
        }
        [RelayCommand] public void Playlist_View(Playlist playlist)
        {
            // Verificamos si hay playlist
            if (playlist is null) return;

            // Verificamos si la lista es la misma 
            if (playlist.Id == Player.SelectedPlaylist.Id)
            {
                CurrentView = new Android_View_SelectedPlaylist();      // Establecemos vista de playlist seleccionada
            }
            else
            {
                Player.ViewPlaylist = playlist;                         // Establecemos nueva vista
                CurrentView = new Android_View_ViewPlaylist();          // Establecemos vista de playlist temporal
            }

        }
        public async Task Playlist_Play(Playlist playlist, string songid = "", int songindex = 0)
        {
            // Verificamos si hay playlist
            if (playlist is null || playlist.Items is null || playlist.Items.Count == 0)
            {
                var title   = "Playlist vacía";
                var message = "No hay canciones en la playlist seleccionada.";
                var ok      = "Aceptar";
                await DialogHelpers.DisplayMessage(title, message, ok);
                return;
            }

            // Cambiamos el estado de las canciones
            foreach (var item in Playlists)
            {
                if (item.Id != playlist.Id)
                {
                    item.IsPlaying = false;
                }
            }

            // Establecemos la playlist seleccionada
            playlist.IsPlaying = true;

            // Configuramos reproductor
            Player.SelectedPlaylist = playlist;
            Player.CurrentSongIndex = songindex;

            // Cambiamos de vista
            CurrentView = new Android_View_SelectedPlaylist();

            // Reproducimos 
            if (playlist.Items[0].Id is not null) 
                if (string.IsNullOrEmpty(songid))
                    await Play(playlist.Items[0].Id!); 
                else
                    await Play(songid);

        }


        // Funciones auxiliares
        public int Playlist_GetTotalSong => Playlists.Sum(p => p.Items?.Count ?? 0);
        public TimeSpan Playlists_GetTotalDuration => Playlists.Aggregate(TimeSpan.Zero, (total, playlist) => total + playlist.Items.Aggregate(TimeSpan.Zero, (subtotal, song) => subtotal + song.Duration.GetValueOrDefault()));


        // =============================================================================================
        // == Commandos del reproductor de musica
        // =============================================================================================
        [RelayCommand] public void Media_Load(string url)
        {
            // Si la URL es nula o vacía, no hacemos nada
            if (string.IsNullOrEmpty(url))
                return;

            // Si ya hay una canción
            Player.CurrentMediaState = MediaElementState.Playing;
            Player.Controller.Source = url;
            Player.Controller.Play();
        }
        [RelayCommand] public void Media_Play()
        {
            if (Player.Controller.CurrentState == MediaElementState.Playing)
            {
                Player.Controller.Pause();
                Player.CurrentMediaState = MediaElementState.Paused;
            }
            else if (Player.Controller.CurrentState == MediaElementState.Paused || Player.Controller.CurrentState == MediaElementState.Stopped)
            {
                Player.Controller.Play();
                Player.CurrentMediaState = MediaElementState.Playing;
            }
        }
        [RelayCommand] public void Media_Stop()
        {
            if (Player.Controller.CurrentState == MediaElementState.Playing)
                Player.Controller.Stop();
        }
        [RelayCommand] public void Media_Forward()
        {
            if (Player.Controller.CurrentState == MediaElementState.Playing || Player.Controller.CurrentState == MediaElementState.Paused)
            {
                var newPosition = Player.Controller.Position.TotalSeconds + 10;
                if (newPosition < Player.Duration) // Asegurarse de no sobrepasar la duración
                {
                    Player.Controller.SeekTo(TimeSpan.FromSeconds(newPosition));
                }
            }
        }
        [RelayCommand] public void Media_Replay()
        {
            if (Player.Controller.CurrentState == MediaElementState.Playing || Player.Controller.CurrentState == MediaElementState.Paused)
            {
                var newPosition = Player.Controller.Position.TotalSeconds - 10;
                if (newPosition > 0) // Asegurarse de no ir por debajo de 0
                {
                    Player.Controller.SeekTo(TimeSpan.FromSeconds(newPosition));
                }
            }
        }
        [RelayCommand] public void Media_SkipNext()
        {
            // Verificamos que nada este null
            if (Player is null || Player.SelectedPlaylist is null) return;

            // Evitar clics múltiples
            if (IsBlockClick) return;
            IsBlockClick = true;

            // 
            try
            {
                if (Player.CurrentSongIndex < (Player.SelectedPlaylist.Items.Count - 1))
                {
                    // Incremento el index
                    Player.CurrentSongIndex++;

                    // Selecciono la nueva song
                    var nextSong = Player.SelectedPlaylist.Items[Player.CurrentSongIndex];

                    // Verificamos si tiene id
                    if (!string.IsNullOrEmpty(nextSong.Id))
                    {
                        _ = Play(nextSong.Id);
                    }
                }
            }
            finally
            {
                IsBlockClick = false;
            }
        }
        [RelayCommand] public void Media_SkipPrevious()
        {
            // Verificamos que nada este null
            if (Player is null || Player.SelectedPlaylist is null) return;

            // Evitar clics múltiples
            if (IsBlockClick) return;
            IsBlockClick = true;

            // 
            try
            {
                if (Player.CurrentSongIndex > 0)
                {
                    // Incremento el index
                    Player.CurrentSongIndex--;

                    // Selecciono la nueva song
                    var prevSong = Player.SelectedPlaylist.Items[Player.CurrentSongIndex];

                    // Verificamos si tiene id
                    if (!string.IsNullOrEmpty(prevSong.Id))
                    {
                        _ = Play(prevSong.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error SkipPrevious: {ex.Message}");
            }
            finally
            {
                IsBlockClick = false;
            }
        }

        // =============================================================================================
        // == Funciones de solo uso aqui
        // =============================================================================================
        public static string GetRandomMusicText()
        {
            // Declaramos una instancia de Random
            Random _random = new();

            // Lista de textos de música
            var musicTexts = new[]
            {
                "Música Pop",
                "Música Rock",
                "Música Jazz",
                "Top 100 2025",
                "Música Pop",
                "Hits del Momento"
            };

            // Seleccionamos un texto aleatorio
            int index = _random.Next(musicTexts.Length);
            return musicTexts[index];
        }
    }
}
