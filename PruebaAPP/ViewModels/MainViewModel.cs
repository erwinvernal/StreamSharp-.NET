using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PruebaAPP.Objetos.Functions;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Objetos.Services;
using PruebaAPP.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

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
            _youtubeService = new YouTubeService();                                 // Servicio de YouTube
            _favoritosService = new FavoriteServices();                               // Servicio de favoritos
            _playlistService = new PlaylistService();                                // Servicio de playlists
            _dispatcher = dispatcher;                                           // Dispatcher para operaciones asincrónicas

            // Asignamos el reproductor de música
            Player = mediaService;                                                      // Asignar el reproductor de música
            Player.vm = this;                                                           // Asignar el ViewModel al reproductor

            // Cargamos lista de favoritos
            Favoritos = new ObservableCollection<Song>(_favoritosService.GetAll());     // Cargar favoritos desde el servicio
            Playlists = new ObservableCollection<Playlist>(_playlistService.GetAll());  // Cargar playlists desde el servicio

            // Precarga por defecto
            SearchText = "Música";                                                      // Texto de búsqueda por defecto
            _ = Search();                                                               // Iniciar búsqueda por defecto
            SearchText = string.Empty;                                                  // Limpiar el texto de búsqueda

        }

        // =============================================================================================
        // == Declaracion de servicios
        // =============================================================================================
        private readonly YouTubeService _youtubeService;                        // Servicio de YouTube
        private readonly FavoriteServices _favoritosService;                      // Servicio de favoritos
        private readonly PlaylistService _playlistService;                       // Servicio de playlists
        private readonly IDispatcher _dispatcher;                            // Dispatcher para operaciones asincrónicas

        // =============================================================================================
        // == Propiedades de la vista
        // =============================================================================================
        public PlayerViewModel Player { get; }                                      // Reproductor de música

        // =============================================================================================
        // == Listas de datos
        // =============================================================================================
        public ObservableCollection<Song> Favoritos { get; }           // Lista de favoritos
        public ObservableCollection<Playlist> Playlists { get; }           // Lista de playlists
        public ObservableCollection<VideoSearchResult> ListItems { get; set; } = [];// Lista de resultados de búsqueda

        // =============================================================================================
        // == Otras propiedades
        // =============================================================================================
        [ObservableProperty] public partial View? CurrentView { get; set; }         // Vista actual
        [ObservableProperty] public partial string? SearchText { get; set; }        // Texto de búsqueda
        [ObservableProperty] public partial bool IsLoading { get; set; }            // Indicador de carga
        [ObservableProperty] public partial bool IsLoading2 { get; set; }           // Indicador de carga para operaciones secundarias

        // =============================================================================================
        // == Variables de control publicas
        // =============================================================================================
        public bool IsBlockClick { get; set; } = false;                             // Bloqueo de clics múltiples
        public record PlaylistRemoveParam(string PlaylistId, string FavoriteId);    // Parámetros para eliminar canción de playlist

        // =============================================================================================
        // == Variables de control privadas
        // =============================================================================================
        private bool _isPlayingSong = false;                                        // Indicador de reproducción en curso
        private readonly List<VideoSearchResult> _cache = [];                       // Caché de resultados de búsqueda
        private string _currentSearch = string.Empty;                               // Texto de búsqueda actual
        private IAsyncEnumerator<VideoSearchResult>? _searchEnumerator;             // Enumerador de resultados de búsqueda
        private bool _isLoadingMore;                                                // Indicador de carga adicional
        private bool _isSearching;                                                  // Indicador de búsqueda en curso
        private int _loadedCount = 0;                                               // Contador de elementos cargados desde la caché

        // =============================================================================================
        // == Funciones del viewmodel
        // =============================================================================================
        public async Task<bool> DisplayMessage(string title, string message, string ok, string? cancel = null)
        {
            // Obtener la página principal de forma segura
            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage == null) return false;

            if (string.IsNullOrEmpty(cancel))
            {
                // Solo un botón
                await mainPage.DisplayAlert(title, message, ok);
                return true;
            }
            else
            {
                // Dos botones
                bool result = await mainPage.DisplayAlert(title, message, ok, cancel);
                return result;
            }
        }

        // =============================================================================================
        // == Comandos de búsqueda
        // =============================================================================================
        [RelayCommand] public async Task Search()
        {

            // Criterio de cancelación   
            if (_isSearching || string.IsNullOrWhiteSpace(SearchText)) return;

            // Activamos variables
            _isSearching = true;
            IsLoading = true;

            // Comenzamos el campo
            try
            {
                // Declaramos variables de reintentos
                int maxretis = 3;
                int retris = 0;
                bool retry = false;

                // Iniciamos ciclo
                do
                {
                    try
                    {

                        // Preconfiguramos
                        _currentSearch = SearchText.Trim();
                        _loadedCount = 0;
                        _cache.Clear();
                        ListItems.Clear();

                        // Obtenemos listado
                        _searchEnumerator = _youtubeService.GetSearchEnumerator(_currentSearch);

                        // Descargamos lista
                        await LoadMore(10);

                        // Si todo salio bien limpeamos 
                        SearchText = string.Empty;

                    }
                    catch
                    {
                        retris++;
                        if (retris > maxretis) return;
                        await Task.Delay(1000);
                        retry = true;
                    }

                } while (retry);
            }
            finally
            {
                _isSearching = false;
                IsLoading = false;
            }

        }
        [RelayCommand] public async Task LoadMore(int count = 10)
        {
            if (_isLoadingMore || _searchEnumerator == null) return;
            _isLoadingMore = true;

            try
            {
                int loaded = 0;

                // 1️⃣ Consumir cache
                var newItems = new List<VideoSearchResult>();
                while (loaded < count && _loadedCount < _cache.Count)
                {
                    var item = _cache[_loadedCount];
                    if (!ListItems.Contains(item)) newItems.Add(item);
                    loaded++;
                    _loadedCount++;
                }

                if (newItems.Count > 0) await _dispatcher.DispatchAsync(() =>
                {
                    foreach (var i in newItems) ListItems.Add(i);
                    return;
                });

                // 2️⃣ Consumir enumerador
                var enumeratorItems = new List<VideoSearchResult>();
                while (loaded < count)
                {
                    int maxretis = 3;
                    int retris = 0;
                    bool retry = false;
                    do
                    {
                        try
                        {
                            if (!await _searchEnumerator.MoveNextAsync()) break;
                            retry = false;
                        }
                        catch
                        {
                            retris++;
                            if (retris > maxretis) return;
                            _searchEnumerator = _youtubeService.GetSearchEnumerator(_currentSearch);
                            await Task.Delay(1000);
                            retry = true;
                        }

                    } while (retry);


                    var item = _searchEnumerator.Current;
                    if (item.Duration != null && item.Duration.Value.TotalMinutes > 1 && !_cache.Contains(item))
                    {
                        _cache.Add(item);
                        enumeratorItems.Add(item);
                        loaded++;
                    }
                }

                if (enumeratorItems.Count > 0)
                    //await _dispatcher.DispatchAsync(() => {
                    foreach (var i in enumeratorItems) ListItems.Add(i);
                //});
            }
            finally
            {
                _isLoadingMore = false;
            }
        }
        [RelayCommand] public async Task PlaySong(VideoSearchResult item)
        {
            // Validamos
            if (_isPlayingSong || item == null)
                return;

            // Cambiamos estado
            _isPlayingSong = true;
            Player.CurrentSong = new Song();
            IsLoading2 = true;

            // Empezamos proceso
            try
            {
                // Detener la canción actual si hay una
                if (Player.CurrentMediaState == MediaElementState.Playing) Player.Controller.Stop();

                // Obtener stream
                IStreamInfo? streamInfo = null;
                try {

                    streamInfo = await Task.Run(() => _youtubeService.GetBestAudioStreamAsync(item.Url));

                } catch (Exception ex) {
                    Debug.WriteLine($"Error al obtener stream: {ex.Message}");
                    return;
                }

                // Verificamos si hay datos
                if (streamInfo is null) return;

                // Configuramos el reproductor
                Player.Controller.MetadataTitle = item.Title;
                Player.Controller.MetadataArtist = item.Author.ChannelTitle;
                Player.Controller.MetadataArtworkUrl = ThumbnailHelper.GetHighestThumbnail(item.Thumbnails);

                // Asignar CurrentSong
                Player.CurrentSong = new Song()
                {
                    Id = item.Id,
                    Title = item.Title,
                    Author = new AuthorSong { ChannelId = item.Author.ChannelId, ChannelTitle = item.Author.ChannelTitle, ChannelUrl = item.Author.ChannelUrl },
                    ThumbnailHighRes = ThumbnailHelper.GetHighestThumbnail(item.Thumbnails),
                    ThumbnailLowRes = ThumbnailHelper.GetLowestThumbnail(item.Thumbnails),
                    Duration = item.Duration,
                    IsFavorite = _favoritosService.Exists(item.Id)
                };

                // Actualizamos
                UpdateSong();

                // Reproducir
                Media_Load(streamInfo.Url);

            } finally {
                IsLoading2 = false;
                _isPlayingSong = false;
            }
        }
        [RelayCommand] public async Task PlaySongById(string id)
        {
            // Validamos
            if (string.IsNullOrWhiteSpace(id)) return;

            // Detener la canción actual si hay una
            if (Player.CurrentMediaState == MediaElementState.Playing) Player.Controller.Stop();

            // Cambiamos estado
            IsLoading2 = true;
            Player.CurrentSong = new Song();

            // Obtener detalles del video
            VideoSearchResult item;
            try {

                // Descargamos manifiesto
                item = await _youtubeService.GetVideoDetailsAsync(id);

                // Lanzamos comando para reproducir
                await PlaySong(item);

            } catch (Exception ex) {
                Debug.WriteLine($"Error al obtener detalles: {ex.Message}");
                return;
            } finally {
                IsLoading2 = false;
            }

        }
        public void UpdateSong()
        {
            foreach (Song favorite in Favoritos)
            {
                if (favorite.Id == Player.CurrentSong?.Id)
                {
                    favorite.IsPlay = true;

                } else {
                    favorite.IsPlay = false;
                }
            }

            foreach (Playlist playlist in Playlists)
            {
                foreach (Song favorite in playlist.Items)
                {
                    if (favorite.Id == Player.CurrentSong?.Id)
                    {
                        favorite.IsPlay = true;

                    } else {
                        favorite.IsPlay = false;
                    }
                }
            }
        }

        // =============================================================================================
        // == Comandos de favoritos
        // =============================================================================================
        [RelayCommand] public void Favorite_Add(Song favorito)
        {
            // Agregamos y comprobamos resultado
            bool result = _favoritosService.Add(favorito);
            if (result)
                Favoritos.Add(favorito);

            // Comprobamos si esta sonando
            if (favorito.Id == Player.CurrentSong?.Id)
            { Player.CurrentSong?.IsFavorite = true; }

            // Mostramos si esta sonando
            UpdateSong();

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
            var result  = await DisplayMessage(title, message, ok, cancel);
            if (!result) return;

            // Eliminar del servicio
            _favoritosService.Delete(id);

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
            // Obtener la página principal de forma segura y sin usar la propiedad obsoleta MainPage
            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage == null)
                return;

            // Creamos entrad de inputbox
            string nombre = await mainPage.DisplayPromptAsync(
                "Nueva Playlist",           // Título
                "Ingresa el nombre:",       // Mensaje
                "Crear",                    // Texto del botón aceptar
                "Cancelar",                 // Texto del botón cancelar
                "Nombre de la playlist",    // Placeholder
                maxLength: 50,              // Límite de caracteres
                keyboard: Keyboard.Text     // Tipo de teclado
            );

            // Verificamos y creamos
            if (string.IsNullOrWhiteSpace(nombre)) return;

            // Llamas al servicio que crea la playlist
            Playlist playlist = _playlistService.Create(nombre);
            Playlists.Add(playlist);

            // Notificamos cambio
            OnPropertyChanged(nameof(Playlists));
        }
        [RelayCommand] public async Task Playlist_ToAdd(Song fav)
        {
            // Preparamos array de titulos
            var playlistTitlesArray = _playlistService.GetAll().Select(p => p.Title).ToArray();

            // Obtener la página principal de forma segura y sin usar la propiedad obsoleta MainPage
            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage == null) return;

            // Verficamos si hay lista
            if (playlistTitlesArray.Length == 0) return;

            // Abrimos cuadro de dialogo
            var result = await mainPage.DisplayActionSheet("Selecciona playlist", "Cancelar", null, playlistTitlesArray);
            if (result != "Cancelar")
            {
                _playlistService.AddFavorite(result, fav);
            }

            // Notificamos cambios
            OnPropertyChanged(nameof(Playlist_GetTotalSong));
            OnPropertyChanged(nameof(Playlists_GetTotalDuration));

        }
        [RelayCommand] public async Task Playlist_Delete(string playlistId)
        {
            // Pedimos confirmacion
            var title   = "¿Eliminar esta playlist?";
            var message = "Esta acción no se puede deshacer. ¿Estás seguro de continuar?";
            var ok      = "Eliminar";
            var cancel  = "Atrás";
            var result  = await DisplayMessage(title, message, ok, cancel);
            if (!result) return;

            // Eliminar del servicio
            _playlistService.Delete(playlistId);

            // Eliminar de la lista observable del ViewModel
            var item = Playlists.FirstOrDefault(p => p.Id == playlistId);
            if (item != null) Playlists.Remove(item);

            // Notificamos cambios
            OnPropertyChanged(nameof(Playlist_GetTotalSong));
            OnPropertyChanged(nameof(Playlists_GetTotalDuration));
        }
        [RelayCommand] public async Task Playlist_ToDelete(PlaylistRemoveParam param)
        {
            // Verificamos si hay parametro
            if (param == null) return;

            // Pedimos confirmacion
            string title   = "¿Quitar de la playlist?";
            string message = "No podrás revertir esta acción. ¿Deseas continuar?";
            string accept  = "Quitar";
            string cancel  = "Cancelar";
            bool result    = await DisplayMessage(title, message, cancel, accept);
            if (!result) return;

            // Procesamos eliminacion
            var pl = Playlists.FirstOrDefault(p => p.Id == param.PlaylistId);
            if (pl == null) return;

            var song = pl.Items.FirstOrDefault(s => s.Id == param.FavoriteId);
            if (song == null) return;

            _playlistService.RemoveFavorite(param.PlaylistId, param.FavoriteId);
            pl.Items.Remove(song);

            // Notificamos cambios
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
            var result  = await DisplayMessage(title, message, ok, cancel);
            if (!result) return;

            // Eliminamos
            _playlistService.ClearAll();
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
        [RelayCommand] public async Task Playlist_Play(Playlist playlist)
        {
            // Verificamos si hay playlist
            if (playlist is null || playlist.Items is null || playlist.Items.Count == 0)
            {
                await DisplayMessage("Playlist", "No hay canciones en la playlist seleccionada.", "Aceptar");
                return;
            }

            // Establecemos en reproducción
            foreach (var item in Playlists)
            {
                if (item.Id != playlist.Id)
                {
                    item.IsPlaying = false;
                }
            }
            playlist.IsPlaying = true;

            // Configuramos reproductor
            Player.SelectedPlaylist = playlist;
            Player.ViewPlaylist = new Playlist();
            Player.CurrentSongIndex = 0;


            // Cambiamos de vista
            CurrentView = new Android_View_SelectedPlaylist();

            // Reproducimos 
            if (playlist.Items[0].Id is not null) 
                await PlaySongById(playlist.Items[0].Id!);

            // Notificamos cambios
            OnPropertyChanged(nameof(Playlists));

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
            Player.Controller.Source = url;
            Player.Controller.Play();
            Player.CurrentMediaState = MediaElementState.Playing;
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
                        _ = PlaySongById(nextSong.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error SkipNext: {ex.Message}");
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
                        _ = PlaySongById(prevSong.Id);
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

    }
}
