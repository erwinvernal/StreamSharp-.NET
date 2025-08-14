using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Timers;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;
using static System.Net.Mime.MediaTypeNames;

namespace PruebaAPP.Views.Android
{
    public partial class Android_Property : ObservableObject
    {

        // Almacenamiento Publico
            public readonly System.Timers.Timer Refresh = new(500);
            public MediaElement? mPlayer;
            
        // Almacenamiento Privado
            private bool IsSearch;
            private bool _isLoadingMore;
            private IAsyncEnumerator<VideoSearchResult>? _searchEnumerator;
            private int _loadedCount = 0; // cuántos items ya agregamos
            private string _currentSearch = ""; // última búsqueda
            private List<VideoSearchResult> _cache = [];

        public Android_Property() 
            {

                // Enlzamos evento del timer
                    Refresh.Elapsed += Refresh_Elapsed;
            }

        // Propiedades de binding
            public ObservableCollection<VideoSearchResult>? ListItems { get; set; } = [];

            [ObservableProperty]
            public partial string? SearchText { get; set; }

            [ObservableProperty]
            public partial string? TitleSong { get; set; }
            
            [ObservableProperty]
            public partial string? Channel { get; set; }
            
            [ObservableProperty]
            public partial string? Thumbnails { get; set; }
            
            [ObservableProperty]
            public partial TimeSpan? CurrentTime {  get; set; }
            
            [ObservableProperty]
            public partial TimeSpan? TotalTime {  get; set; }
            
            [ObservableProperty]
            public partial double ProgressTime { get; set; }

        // Funciones del timer
        private async void Refresh_Elapsed(object? sender, ElapsedEventArgs e)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        // Verificamos si no mandamos un error
                            if (mPlayer is null) { throw new Exception("Al parecer no se inicializo el reproductor."); };

                        // Actualizamos valores
                            CurrentTime  = mPlayer.Position;
                            TotalTime    = mPlayer.Duration;
                            if (CurrentTime > TimeSpan.Zero && mPlayer.Duration > TimeSpan.Zero)
                            {
                                ProgressTime = mPlayer.Position.TotalSeconds / mPlayer.Duration.TotalSeconds;
                            } else {
                                ProgressTime = 0.0f;
                            }
                    });
                }

        // Funciones de busqueda
            // Command action
                [RelayCommand]
                public async Task LoadMore() => await LoadMoreInternal(10);

                [RelayCommand]
                public async Task Search() => await SearchInternal(SearchText!);

                [RelayCommand]
                public async Task ItemSelectionChanged(VideoSearchResult item)
                {
                    if (item == null) return;
                    await PlaySong(item);
                }

        // Funciones internas
            public async Task<bool> SearchInternal(string search)
            {
                if (IsSearch) return false;
                IsSearch = true;

                try
                {
                    if (!string.IsNullOrWhiteSpace(SearchText))
                        search = SearchText.Trim();

                    _currentSearch = search;
                    _loadedCount = 0;

                    _cache.Clear();
                    await MainThread.InvokeOnMainThreadAsync(() => ListItems!.Clear());

                    // Creamos enumerador una sola vez
                    var youtube = new YoutubeClient();
                    _searchEnumerator = youtube.Search.GetVideosAsync(_currentSearch).GetAsyncEnumerator();

                    // Cargamos primera tanda
                    await LoadMoreInternal(10);
                }
                catch (Exception ex)
                {
                    DebugError(ex);
                    return false;
                }
                finally
                {
                    IsSearch = false;
                }

                return true;
            }
            public async Task LoadMoreInternal(int count)
            {
                if (_isLoadingMore || _searchEnumerator == null) return;
                _isLoadingMore = true;

                try
                {
                    int loaded = 0;

                    // Primero consumimos cache
                    while (loaded < count && _loadedCount < _cache.Count)
                    {
                        var item = _cache[_loadedCount];
                        await MainThread.InvokeOnMainThreadAsync(() => ListItems!.Add(item));
                        loaded++;
                        _loadedCount++;
                    }

                    // Luego seguimos con enumerador si hace falta
                    while (loaded < count)
                    {
                        if (!await _searchEnumerator.MoveNextAsync()) break;

                        var item = _searchEnumerator.Current;
                        if (item.Duration != null)
                        {
                            _cache.Add(item); // agregamos al cache
                            await MainThread.InvokeOnMainThreadAsync(() => ListItems!.Add(item));
                            loaded++;
                            // Nota: NO incrementamos _loadedCount aquí, solo cuando se consuma cache
                        }
                    }
                }
                finally
                {
                    _isLoadingMore = false;
                }
            }
            public async Task PlaySong(VideoSearchResult item)
                        {
                            // Reiniciamos controles
                                // Reproductor
                                    mPlayer!.Stop();
                                    mPlayer.Source = null;
            
                                // Propiedades
                                    Thumbnails  = null;
                                    TitleSong   = "Recibiendo informacion...";
                                    Channel     = "Espere por favor...";
                                    CurrentTime = TimeSpan.Zero;
                                    TotalTime   = TimeSpan.Zero;
            
                            // Preparación de la funcion
                                IStreamInfo? StreamInfo = null;
                                StreamManifest? StreamManifest = null;
            
                            // Variables de la funcion
                                var youtube = new YoutubeClient();  // Inicializacion
                                var link = item.Url;                // Url
            
                            // Evitar NetworkOnMainThreadException
                                try {
                                    await Task.Run(async () => {
                                        StreamManifest = await youtube.Videos.Streams.GetManifestAsync(link);
                                        StreamInfo = StreamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                                    });
                                } catch (Exception ex) {
                                    Debug.WriteLine(ex.Message);
                                    return;
                                }
            
                            // Ejecutamos Audio
                                if (StreamInfo is null) { return; }
                                if (StreamManifest is null) { return; }
            
                            // Establecemos informacion
                                Thumbnails  = item.Thumbnails[0].Url;
                                TitleSong   = item.Title;
                                Channel     = item.Author.ChannelTitle;


                                // Reproducimos
                                mPlayer.Source = StreamInfo.Url;
                                mPlayer.Play();
            
                        }
                

        // Funciones de debugaccion
            bool DebugError(Exception ex)
            {
                return true;
            }

    }
}
