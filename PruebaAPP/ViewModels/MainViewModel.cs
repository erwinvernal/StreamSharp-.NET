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

namespace PruebaAPP.Views.Android.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {


        // =============================================================================================
        // == Inicializacion del contructor
        // =============================================================================================
            public MainViewModel(PlayerViewModel mediaService, IDispatcher dispatcher)
                {

                    // Inicializamos
                    _favoritosService   = new FavoriteServices();
                    _youtubeService     = new YouTubeService();
                    _playlistService    = new PlaylistService();
                    _dispatcher         = dispatcher;

                    // Inicializaciones de ViewModel
                    Player = mediaService;

                    // Cargamos lista de favoritos
                    Favoritos = new ObservableCollection<Song>(_favoritosService.GetAll());
                    LPlaylist = new ObservableCollection<Playlist>(_playlistService.GetAll());

                    // Precarga por defecto
                    SearchText = "Música";
                    _ = Search();
                    SearchText = string.Empty;

                }

        // =============================================================================================
        // == Declaracion de servicios
        // =============================================================================================
            private readonly YouTubeService     _youtubeService;
            private readonly FavoriteServices   _favoritosService;
            private readonly PlaylistService    _playlistService;
            private readonly IDispatcher        _dispatcher;

        // =============================================================================================
        // == Propiedades de la vista
        // =============================================================================================
            public PlayerViewModel Player { get; }


        // =============================================================================================
        // == Lista de favoritos y playlists
        // =============================================================================================
            public ObservableCollection<Song> Favoritos { get; }
            public ObservableCollection<Playlist> LPlaylist { get; }
            public ObservableCollection<VideoSearchResult> ListItems { get; set; } = [];

        // =============================================================================================
        // == Otras propiedades
        // =============================================================================================
            [ObservableProperty] public partial View? CurrentView { get; set; }
            [ObservableProperty] public partial string? SearchText { get; set; }
            [ObservableProperty] public partial bool IsLoading { get; set; }
            [ObservableProperty] public partial bool IsLoading2 { get; set; }

        // =============================================================================================
        // == Buscar
        // =============================================================================================
            // Declaracion de la lista de resultados
            private bool _isPlayingSong = false;
            private readonly List<VideoSearchResult> _cache = [];
            private string _currentSearch = string.Empty;
            private IAsyncEnumerator<VideoSearchResult>? _searchEnumerator;
            private bool _isLoadingMore;
            private bool _isSearching;
            private int _loadedCount = 0;

            // Comandos de busqueda
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
                        Id               = item.Id,
                        Title            = item.Title,
                        Author           = new AuthorSong { ChannelId = item.Author.ChannelId, ChannelTitle = item.Author.ChannelTitle, ChannelUrl = item.Author.ChannelUrl},
                        ThumbnailHighRes = ThumbnailHelper.GetHighestThumbnail(item.Thumbnails),
                        ThumbnailLowRes  = ThumbnailHelper.GetLowestThumbnail(item.Thumbnails),
                        Duration         = item.Duration,
                        IsFavorite       = _favoritosService.Exists(item.Id)
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
                IsLoading2  = true;
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

                foreach (Playlist playlist in LPlaylist)
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
        // == Favoritos
        // =============================================================================================
            // Funciones para manejar favoritos
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
                // Obtener la página principal de forma segura y sin usar la propiedad obsoleta MainPage
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage == null) return;

                // Pedimos confirmacion
                bool result = await mainPage.DisplayAlert("Quitar de favoritos?", "Quieres eliminar de favoritos?", "Eliminar", "Cancelar");
                if (!result) return;

                // Eliminar del servicio
                _favoritosService.Delete(id);

                // Eliminar de la lista observable del ViewModel
                var item = Favoritos.FirstOrDefault(f => f.Id == id);
                if (item != null) Favoritos.Remove(item);

                // Verificamos si la cancion esta corriendo
                if (Player.CurrentSong != null && !string.IsNullOrEmpty(Player. CurrentSong.Id) && item != null && !string.IsNullOrEmpty(item.Id) && Player.CurrentSong.Id == item.Id)
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
                    Id    = "0",
                    Title = "Mis lista de favoritos",
                    Items = Favoritos
                };

                // Establecemos playlist
                Player.CurrentPlaylist = playlist;

                // Abrimos ventana
                CurrentView = new Android_View_Playlist();

            }
            
            public TimeSpan Favorite_GetTotalD => Favoritos.Aggregate(TimeSpan.Zero, (total, song) => total + song.Duration.GetValueOrDefault());

        // =============================================================================================
        // == PlayList
        // =============================================================================================
        public record PlaylistRemoveParam(string PlaylistId, string FavoriteId);
        
        // Comandos de control de playlist
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
                LPlaylist.Add(playlist);

                // Notificamos cambio
                OnPropertyChanged(nameof(LPlaylist));
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
                // Obtener la página principal de forma segura y sin usar la propiedad obsoleta MainPage
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage == null) return;

                // Pedimos confirmacion
                bool result = await mainPage.DisplayAlert("¿Eliminar playlist?", "Esta acción no se puede restaurar. ¿Estas seguro?", "Eliminar", "Cancelar");
                if (!result) return;

                // Eliminar del servicio
                _playlistService.Delete(playlistId);

                // Eliminar de la lista observable del ViewModel
                var item = LPlaylist.FirstOrDefault(p => p.Id == playlistId);
                if (item != null)
                LPlaylist.Remove(item);

                // Notificamos cambios
                OnPropertyChanged(nameof(Playlist_GetTotalSong));
                OnPropertyChanged(nameof(Playlists_GetTotalDuration));
            }
        [RelayCommand] public async Task Playlist_ToDelete(PlaylistRemoveParam param)
            {
                // Verificamos si hay parametro
                if (param == null) return;

                // Buscamos page principal
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage is null) return;

                // Pedimos confirmacion
                string title        = "¿Quitar de esta playlist?";
                string description  = "Esta acción no se puede restaurar. ¿Estás seguro de continuar?";
                string accept       = "Quitar";
                string cancel       = "Cancelar";
                bool result         = await mainPage.DisplayAlert(title, description, cancel, accept);
                if (!result) return;

                // Procesamos eliminacion
                var pl = LPlaylist.FirstOrDefault(p => p.Id == param.PlaylistId);
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
                if (LPlaylist.Count == 0) return;

                // Obtener la página principal de forma segura y sin usar la propiedad obsoleta MainPage
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage == null) return;

                // Pedimos confirmacion
                bool result = await mainPage.DisplayAlert("¿Eliminar toda la lista?", "Esta acción no se puede restaurar. ¿Estas seguro de querer continuar?", "Eliminar", "Cancelar");
                if (!result) return;

                // Eliminamos
                _playlistService.ClearAll();
                LPlaylist.Clear();

                //Notificamos cambio
                OnPropertyChanged(nameof(LPlaylist));
                OnPropertyChanged(nameof(Playlist_GetTotalSong));
                OnPropertyChanged(nameof(Playlists_GetTotalDuration));
            }
        [RelayCommand] public void Playlist_Play(Playlist playlist)
        {
            // Verificamos si hay playlist
            if (playlist is null) return;

            // Establecemos la playlist actual
            Player.CurrentPlaylist = playlist;

            // Abrimos ventana
            CurrentView = new Android_View_Playlist();

            // Notificamos cambios
            OnPropertyChanged(nameof(LPlaylist));

        }

        // Funciones auxiliares
        public int Playlist_GetTotalSong => LPlaylist.Sum(p => p.Items?.Count ?? 0);
            public TimeSpan Playlists_GetTotalDuration => LPlaylist.Aggregate(TimeSpan.Zero, (total, playlist) => total + playlist.Items.Aggregate(TimeSpan.Zero, (subtotal, song) => subtotal + song.Duration.GetValueOrDefault()));


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
            if (Player is null || Player.CurrentPlaylist is null) return;

            // Evitar clics múltiples
            if (Player.IsBlockClick) return;
            Player.IsBlockClick = true;

            // 
            try
            {
                if (Player.CurrentSongIndex < (Player.CurrentPlaylist.Items.Count - 1))
                {
                    // Incremento el index
                    Player.CurrentSongIndex++;

                    // Selecciono la nueva song
                    var nextSong = Player.CurrentPlaylist.Items[Player.CurrentSongIndex];

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
                Player.IsBlockClick = false;
            }
        }
        [RelayCommand] public void Media_SkipPrevious()
        {
            // Verificamos que nada este null
            if (Player is null || Player.CurrentPlaylist is null) return;

            // Evitar clics múltiples
            if (Player.IsBlockClick) return;
            Player.IsBlockClick = true;

            // 
            try
            {
                if (Player.CurrentSongIndex > 0)
                {
                    // Incremento el index
                    Player.CurrentSongIndex--;

                    // Selecciono la nueva song
                    var prevSong = Player.CurrentPlaylist.Items[Player.CurrentSongIndex];

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
                Player.IsBlockClick = false;
            }
        }


    }
}
