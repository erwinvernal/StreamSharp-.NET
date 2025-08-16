using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Objetos.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace PruebaAPP.Views.Android.ViewModels
{
    public partial class AndroidPropertyViewModel : ObservableObject
    {
        // Declaracion de servicios
            private readonly YouTubeService _youtubeService;
            private readonly MediaPlayerService _mediaService;
            private readonly FavoriteServices _favoritosService;
            private readonly IDispatcher _dispatcher;

        // Declaracion de eventos
            public event Func<string, string, string?, string?, Task>? ShowMessage;

        // Declaracion de variables
            private bool _isSearching;
            private bool _isLoadingMore;
            private IAsyncEnumerator<VideoSearchResult>? _searchEnumerator;
            private int _loadedCount = 0;
            private string _currentSearch = string.Empty;
            private readonly List<VideoSearchResult> _cache = [];
            private CancellationTokenSource? _cts;

        // Lista de coleccion
            public ObservableCollection<VideoSearchResult> ListItems { get; set; } = [];
            public ObservableCollection<Favorite> Favoritos { get; } = [];

        // Propiedades de la cancion seleccionada
            [ObservableProperty] public partial Song? CurrentSong { get; set; }

        // Otras propiedades
            [ObservableProperty] public partial string?   SearchText    { get; set; }
            [ObservableProperty] public partial bool      IsLoading     { get; set; }
            [ObservableProperty] public partial bool      IsLoading2     { get; set; }

        // Inicializacion del contructor
            public AndroidPropertyViewModel(MediaPlayerService mediaService, IDispatcher dispatcher)
            {

                // Inicializamos
                _favoritosService = new FavoriteServices();
                _youtubeService   = new YouTubeService();
                _mediaService     = mediaService;
                _dispatcher       = dispatcher;

                // Creamos un Currentsong vacio
                CurrentSong = new Song()
                {
                    CurrentTime = TimeSpan.Zero,
                    TotalTime = TimeSpan.Zero,
                    ProgressTime = 1
                };

                // Cargamos lista de favoritos
                Favoritos = new ObservableCollection<Favorite>(_favoritosService.GetAll());

                // Precarga por defecto
                SearchText = "Música";
                _ = Search();
                SearchText = string.Empty;

            }

        // Commandos de buscar
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
                        try {
                
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
                
                        } catch {
                            retris++;
                            if (retris > maxretis) return;
                            await Task.Delay(1000);
                                retry = true;
                        }
                
                    } while (retry);
                } finally {
                    _isSearching = false;
                    IsLoading = false;
                }
                    
            }
            [RelayCommand] public async Task PlaySong(VideoSearchResult item)
            {
                _cts?.Cancel();             
                _cts = new CancellationTokenSource();
                var token = _cts.Token;

                if (item == null) return;

                IsLoading2 = true;
                _mediaService.Stop();

                CurrentSong = new Song()
                {
                    Title = "Cargando...",
                    Channel = "Por favor espere...",
                    CurrentTime = TimeSpan.Zero,
                    TotalTime = TimeSpan.Zero,
                    ProgressTime = 1
                };

                IStreamInfo? streamInfo = null;

                try
                {
                    // Ejecutamos la tarea y la asociamos al token de cancelación
                    var task = Task.Run(() => _youtubeService.GetBestAudioStreamAsync(item.Url));

                    // Esperamos que termine o que se cancele
                    streamInfo = await Task.WhenAny(task, Task.Delay(-1, token)) == task
                        ? await task
                        : null;

                    token.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Reproducción cancelada.");
                    return;
                }
                catch (Exception ex)
                {
                    if (ShowMessage != null)
                        await ShowMessage.Invoke("Ups..", "Algo salió mal, inténtalo de nuevo.", "OK", null);
                    Debug.WriteLine(ex.Message);
                    return;
                }
                finally
                {
                    IsLoading2 = false;
                }

                if (streamInfo == null) return;

                CurrentSong = new Song()
                {
                    Id = item.Id,
                    Title = item.Title,
                    Channel = "@" + item.Author.ChannelTitle,
                    Thumbnails = item.Thumbnails[0].Url,
                    Duration = item.Duration,
                    CurrentTime = TimeSpan.Zero,
                    TotalTime = TimeSpan.Zero,
                    ProgressTime = 0,
                    Favorite = _favoritosService.Exists(item.Id)
                };

                _mediaService.Play(streamInfo.Url);
            }
            [RelayCommand] public async Task PlaySongById(string id)
            {

                // Verificamos si el ID es válido
                if (string.IsNullOrWhiteSpace(id))
                    return;

                // Cambiamos estado
                IsLoading2 = true;
                CurrentSong = new Song()
                {
                    Title = "Cargando...",
                    Channel = "Recibiendo datos..."
                };

                // Buscamos manifiesto correctamente usando await
                VideoSearchResult item = await _youtubeService.GetVideoDetailsAsync(id);

                // Llamamos al metodo original
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
                        await _dispatcher.DispatchAsync(() => {
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
                            } catch {
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


        // Commandos de favoritos
            [RelayCommand] public void AddFavorite(Favorite favorito)
            {
                _favoritosService.Add(favorito);
                Favoritos.Add(favorito);
            }
            [RelayCommand] public void DeleteFavorite(string id)
            {
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

    }
}
