using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Objetos.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace PruebaAPP.Views.Android.ViewModels
{
    public partial class AndroidPropertyViewModel : ObservableObject
    {


        // =============================================================================================
        // == Inicializacion del contructor
        // =============================================================================================
            public AndroidPropertyViewModel(MediaPlayerService mediaService, IDispatcher dispatcher)
                {

                    // Inicializamos
                    _favoritosService   = new FavoriteServices();
                    _youtubeService     = new YouTubeService();
                    _playlistService    = new PlaylistService();
                    _mediaService       = mediaService;
                    _dispatcher         = dispatcher;

                    // Cargamos lista de favoritos
                    Favoritos = new ObservableCollection<Objetos.Models.Favorite>(_favoritosService.GetAll());
                    Playlists = new ObservableCollection<Objetos.Models.Playlist>(_playlistService.GetAll());

                    // Precarga por defecto
                    SearchText = "Música";
                    _ = Search();
                    SearchText = string.Empty;

                }

        // =============================================================================================
        // == Declaracion de servicios
        // =============================================================================================
            private readonly YouTubeService     _youtubeService;
            private readonly MediaPlayerService _mediaService;
            private readonly FavoriteServices   _favoritosService;
            private readonly PlaylistService    _playlistService;
            private readonly IDispatcher        _dispatcher;

        // =============================================================================================
        // == Declaracion de variables
        // =============================================================================================
            private bool _isSearching;
            private bool _isLoadingMore;
            private IAsyncEnumerator<VideoSearchResult>? _searchEnumerator;
            private int _loadedCount = 0;
            private string _currentSearch = string.Empty;
            private readonly List<VideoSearchResult> _cache = [];
            private CancellationTokenSource? _cts;

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
            public ObservableCollection<VideoSearchResult> ListItems { get; set; } = [];
            private bool _isPlayingSong = false;

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
            [RelayCommand] public async Task PlaySong(VideoSearchResult item)
            {
                // Validamos
                if (_isPlayingSong || item == null)
                    return;

                // Cambiamos estado
                _isPlayingSong = true;
                CurrentSong = new Song();

                // Empezamos proceso
                try
                {
                    IsLoading2 = true;

                    // Detener la canción actual si hay una
                    if (_mediaService.Player.CurrentState == MediaElementState.Playing)
                        _mediaService.Player.Stop();

                    // Obtener stream
                    IStreamInfo? streamInfo = null;
                    try
                    {
                        streamInfo = await Task.Run(() => _youtubeService.GetBestAudioStreamAsync(item.Url));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error al obtener stream: {ex.Message}");
                        return;
                    }

                    if (streamInfo == null)
                        return;

                    // Asignar CurrentSong
                    CurrentSong = new Song()
                    {
                        Id = item.Id,
                        Title = item.Title,
                        Channel = item.Author,
                        Thumbnails = item.Thumbnails,
                        Duration = item.Duration,
                        CurrentTime = TimeSpan.Zero,
                        TotalTime = item.Duration ?? TimeSpan.Zero,
                        ProgressTime = 0,
                        Favorite = _favoritosService.Exists(item.Id)
                    };

                    // Reproducir
                    _mediaService.Play(streamInfo.Url);

                } finally {
                    IsLoading2 = false;
                    _isPlayingSong = false;
                }
            }
            [RelayCommand] public async Task PlaySongById(string id)
            {
                if (string.IsNullOrWhiteSpace(id))
                    return;

                // Obtener detalles del video
                VideoSearchResult item;
                try
                {
                    item = await _youtubeService.GetVideoDetailsAsync(id);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al obtener detalles: {ex.Message}");
                    return;
                }

                await PlaySong(item);
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
                    if (newItems.Count > 0)
                        await _dispatcher.DispatchAsync(() =>
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
            public void UpdateProgress()
            {
                CurrentSong?.CurrentTime = _mediaService.CurrentTime;
                CurrentSong?.TotalTime = _mediaService.TotalTime;

                if (CurrentSong?.CurrentTime > TimeSpan.Zero && CurrentSong?.TotalTime > TimeSpan.Zero)
                {
                    CurrentSong?.ProgressTime = CurrentSong.CurrentTime.Value.TotalSeconds / CurrentSong.TotalTime.Value.TotalSeconds;
                }
                else
                {
                    CurrentSong?.ProgressTime = 0.0;
                }
            }


        // =============================================================================================
        // == Favoritos
        // =============================================================================================
            // Declaración de la colección de favoritos
            public ObservableCollection<Objetos.Models.Favorite> Favoritos { get; } = [];

            // Funciones para manejar favoritos
            [RelayCommand] public void AddFavorite(Objetos.Models.Favorite favorito)
            {
                _favoritosService.Add(favorito);
                Favoritos.Add(favorito);
            }
            [RelayCommand] public async Task DeleteFavorite(string id)
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
                if (item != null)
                    Favoritos.Remove(item);

                // Verificamos si la cancion esta corriendo
                if (CurrentSong != null && !string.IsNullOrEmpty(CurrentSong.Id) && item != null && !string.IsNullOrEmpty(item.Id) && CurrentSong.Id == item.Id)
                {
                    CurrentSong.Favorite = false;
                }

            }
            [RelayCommand] public void ChangeFavoriteType(FType newType)
            {

                // Traer filtrados desde el servicio
                var filtrados = _favoritosService.GetByType(newType);

                // Refrescar la colección sin reemplazarla
                Favoritos.Clear();
                foreach (var fav in filtrados)
                    Favoritos.Add(fav);

            }

        // =============================================================================================
        // == PlayList
        // =============================================================================================
            // Declaración de la colección de playlists
            public ObservableCollection<Playlist> Playlists { get; } = [];
            public record PlaylistRemoveParam(string PlaylistId, string FavoriteId);

            // Propiedades de control de playlist
            [ObservableProperty] public partial Playlist? SelectedPlaylist { get; set; }
            
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
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    // Llamas al servicio que crea la playlist
                    var playlist = _playlistService.Create(nombre);
                    Playlists.Add(playlist);
                }

            }
            [RelayCommand] public async Task Playlist_ToAdd(Favorite fav)
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

            }
            [RelayCommand] public async Task Playlist_Delete(string playlistId)
            {
                // Obtener la página principal de forma segura y sin usar la propiedad obsoleta MainPage
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage == null) return;

                // Pedimos confirmacion
                bool result = await mainPage.DisplayAlert("¿Quitar playlist?", "Esta acción no se puede restaurar. ¿Estas seguro de querer continuar?", "Eliminar", "Cancelar");
                if (!result) return;

                // Eliminar del servicio
                _playlistService.Delete(playlistId);

                // Eliminar de la lista observable del ViewModel
                var item = Playlists.FirstOrDefault(p => p.Id == playlistId);
                if (item != null)
                Playlists.Remove(item);
            }
            [RelayCommand] public async Task Playlist_ToDelete(PlaylistRemoveParam param)
            {
                if (param == null) return;

                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage == null) return;

                bool result = await mainPage.DisplayAlert(
                    "¿Quitar de esta playlist?",
                    "Esta acción no se puede restaurar. ¿Estás seguro de continuar?",
                    "Quitar", "Cancelar");
                if (!result) return;

                var pl = Playlists.FirstOrDefault(p => p.Id == param.PlaylistId);
                if (pl == null) return;

                var song = pl.Items.FirstOrDefault(s => s.Id == param.FavoriteId);
                if (song == null) return;

                _playlistService.RemoveFavorite(param.PlaylistId, param.FavoriteId);
                pl.Items.Remove(song);
            }
            [RelayCommand] public async Task Playlist_ClearAll()
            {
                // Verificamos si hay playlists
                if (Playlists.Count == 0) return;

                // Obtener la página principal de forma segura y sin usar la propiedad obsoleta MainPage
                var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
                if (mainPage == null) return;

                // Pedimos confirmacion
                bool result = await mainPage.DisplayAlert("¿Eliminar toda la lista?", "Esta acción no se puede restaurar. ¿Estas seguro de querer continuar?", "Eliminar", "Cancelar");
                if (!result) return;

                // Eliminamos
                _playlistService.ClearAll();
                Playlists.Clear();
            }

        // =============================================================================================
        // == Controles de mediap
        // =============================================================================================
            // Declaracion de variables
            public bool HasCurrentSong => CurrentSong != null;
            public int CurrentSongIndex;

            // Propiedades de control de media
            [ObservableProperty] public partial MediaElementState CurrentMediaState { get; set; }
            [ObservableProperty] public partial Song? CurrentSong { get; set; } = null;

            // Comandos de control de media
            [RelayCommand] public void MediaP_Play()
            {
                if (CurrentMediaState == MediaElementState.Playing) {
                    _mediaService.Player.Pause();
                } else { 
                    _mediaService.Player.Play();
                }
            }
            [RelayCommand] public void MediaP_Stop()
            {
                //_mediaService.Player.Stop();
                //_mediaService.Player.Source = null;
            }
            [RelayCommand] public void MediaP_Replay()
            {

                // Verificamos si hay cancion actual
                if (_mediaService.Player == null || CurrentSong == null) return;

                // Tomamos el tiempo actual y sumamos 10 segundos
                var newPosition = _mediaService.Player.Position + TimeSpan.FromSeconds(10);

                // Nos aseguramos de no pasarnos de la duración total
                if (CurrentSong.TotalTime.HasValue && newPosition > CurrentSong.TotalTime.Value)
                    newPosition = CurrentSong.TotalTime.Value;

                // Usar el método SeekTo en lugar de asignar directamente la propiedad Position
                _ = _mediaService.Player.SeekTo(newPosition);
            }
            [RelayCommand] public void MediaP_Forward()
            {

                // Verificamos si hay cancion actual
                if (_mediaService.Player == null || CurrentSong == null) return;

                // Tomamos el tiempo actual y restamos 10 segundos
                var newPosition = _mediaService.Player.Position - TimeSpan.FromSeconds(10);

                // Nos aseguramos de no pasarnos de la duración total
                if (CurrentSong.TotalTime.HasValue && newPosition > CurrentSong.TotalTime.Value)
                    newPosition = CurrentSong.TotalTime.Value;

                // Usar el método SeekTo en lugar de asignar directamente la propiedad Position
                _ = _mediaService.Player.SeekTo(newPosition);
            }
            [RelayCommand] public async Task MediaP_SkipNext()
            {
                try
                {
                    if (SelectedPlaylist != null && CurrentSongIndex < SelectedPlaylist.Items.Count - 1)
                    {
                        CurrentSongIndex++;
                        var prevSong = SelectedPlaylist.Items[CurrentSongIndex];
                        if (!string.IsNullOrEmpty(prevSong.Id))
                            await PlaySongById(prevSong.Id);
                    }
                    else
                    {
                        // O reinicia playlist o deja en estado detenido
                        CurrentSongIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error MediaEnded: {ex.Message}");
                }
            }
            [RelayCommand] public async Task MediaP_SkipPrevious()
            {
                try
                {
                    if (SelectedPlaylist != null && CurrentSongIndex < SelectedPlaylist.Items.Count - 1)
                    {
                        CurrentSongIndex--;
                        var prevSong = SelectedPlaylist.Items[CurrentSongIndex];
                        if (!string.IsNullOrEmpty(prevSong.Id))
                            await PlaySongById(prevSong.Id);
                    }
                    else
                    {
                        // O reinicia playlist o deja en estado detenido
                        CurrentSongIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error MediaEnded: {ex.Message}");
                }
            }

            // Funciones auxiliares
            partial void OnCurrentSongChanged(Song? value)
            {
                OnPropertyChanged(nameof(HasCurrentSong));
            }

    }
}
