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
                    if (Player.CurrentMediaState == MediaElementState.Playing) Player.Stop();

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
                    Player.Load(streamInfo.Url);

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
                if (Player.CurrentMediaState == MediaElementState.Playing) Player.Stop();

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
            //public void UpdateProgress()
            //{
            //    CurrentSong?.CurrentTime = _mediaService.Player.Position.Microseconds;
            //    CurrentSong?.TotalTime = _mediaService.TotalTime;
            //
            //    if (CurrentSong?.CurrentTime > TimeSpan.Zero && CurrentSong?.TotalTime > TimeSpan.Zero)
            //    {
            //        CurrentSong?.ProgressTime = CurrentSong.CurrentTime.Value.TotalSeconds / CurrentSong.TotalTime.Value.TotalSeconds;
            //    }
            //    else
            //    {
            //        CurrentSong?.ProgressTime = 0.0;
            //    }
            //}
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
            // Declaración de la colección de favoritos
            

            // Funciones para manejar favoritos
            /// <summary>Agrega una <see cref="Song"/> a Favoritos.</summary>
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

            /// <summary>Elimina una <see cref="Song"/> de favoritos.</summary>
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

            /// <summary>Crea una playlist en memoria para reproducir todo el contenido.</summary>
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
                SelectedPlaylist = playlist;

                // Abrimos ventana
                CurrentView = new Android_View_Playlist();

            }
            
            // Funciones auxiliares
            /// <summary> Recupera el tiempo total de la lista de favoritos.</summary>
            public TimeSpan Favorite_GetTotalD => Favoritos.Aggregate(TimeSpan.Zero, (total, song) => total + song.Duration.GetValueOrDefault());

        // =============================================================================================
        // == PlayList
        // =============================================================================================

            public record PlaylistRemoveParam(string PlaylistId, string FavoriteId);

            // Propiedades de control de playlist
            [ObservableProperty] public partial Playlist? SelectedPlaylist { get; set; } = new Playlist();
            
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

            // Funciones auxiliares
            public int Playlist_GetTotalSong => LPlaylist.Sum(p => p.Items?.Count ?? 0);
            public TimeSpan Playlists_GetTotalDuration => LPlaylist.Aggregate(TimeSpan.Zero, (total, playlist) => total + playlist.Items.Aggregate(TimeSpan.Zero, (subtotal, song) => subtotal + song.Duration.GetValueOrDefault()));




    }
}
