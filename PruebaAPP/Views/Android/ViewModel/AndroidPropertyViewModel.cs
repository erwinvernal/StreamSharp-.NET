using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

using PruebaAPP.Views.Android.Services;

namespace PruebaAPP.Views.Android.ViewModels
{
    public partial class AndroidPropertyViewModel : ObservableObject
    {
        private readonly YouTubeService _youtubeService;
        private readonly MediaPlayerService _mediaService;
        private readonly IDispatcher _dispatcher;

        private bool _isSearching;
        private bool _isLoadingMore;
        private IAsyncEnumerator<VideoSearchResult>? _searchEnumerator;
        private int _loadedCount = 0;
        private string _currentSearch = "";
        private readonly List<VideoSearchResult> _cache = new();

        public ObservableCollection<VideoSearchResult> ListItems { get; set; } = new();

        [ObservableProperty] public partial string?   SearchText { get; set; }
        [ObservableProperty] public partial string?   TitleSong { get; set; }
        [ObservableProperty] public partial string?   Channel { get; set; }
        [ObservableProperty] public partial string?   Thumbnails { get; set; }
        [ObservableProperty] public partial TimeSpan? CurrentTime { get; set; }
        [ObservableProperty] public partial TimeSpan? TotalTime { get; set; }
        [ObservableProperty] public partial double    ProgressTime { get; set; }
        [ObservableProperty] public partial bool      IsLoading { get; set; }

        public AndroidPropertyViewModel(
            YouTubeService youtubeService,
            MediaPlayerService mediaService,
            IDispatcher dispatcher)
        {
            _youtubeService = youtubeService;
            _mediaService = mediaService;
            _dispatcher = dispatcher;

            // Precarga por defecto
            SearchText = "Música";
            _ = Search();
            SearchText = string.Empty;
        }

        [RelayCommand]
        public async Task Search()
        {
            if (_isSearching || string.IsNullOrWhiteSpace(SearchText)) return;

            _isSearching = true;
            IsLoading = true;

            try
            {
                _currentSearch = SearchText.Trim();
                _loadedCount = 0;
                _cache.Clear();
                ListItems.Clear();

                _searchEnumerator = _youtubeService.GetSearchEnumerator(_currentSearch);
                await LoadMoreInternal(10);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                _isSearching = false;
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadMore()
        {
            await LoadMoreInternal(10);
        }
        private async Task LoadMoreInternal(int count)
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
                    });

                // 2️⃣ Consumir enumerador
                var enumeratorItems = new List<VideoSearchResult>();
                while (loaded < count)
                {
                    if (!await _searchEnumerator.MoveNextAsync()) break;

                    var item = _searchEnumerator.Current;
                    if (item.Duration != null && !_cache.Contains(item))
                    {
                        _cache.Add(item);
                        enumeratorItems.Add(item);
                        loaded++;
                    }
                }
                if (enumeratorItems.Count > 0)
                    await _dispatcher.DispatchAsync(() => {
                        foreach (var i in enumeratorItems) ListItems.Add(i);
                    });
            }
            finally
            {
                _isLoadingMore = false;
            }
        }


        [RelayCommand]
        public async Task ItemSelectionChanged(VideoSearchResult item)
        {
            if (item == null) return;
            await PlaySong(item);
        }
        public async Task PlaySong(VideoSearchResult item)
        {
            IsLoading = true;
            _mediaService.Stop();

            Thumbnails = null;
            TitleSong = "Cargando información...";
            Channel = "Espere por favor...";
            CurrentTime = TimeSpan.Zero;
            TotalTime = TimeSpan.Zero;

            IStreamInfo? streamInfo = null;

            try
            {
                streamInfo = await Task.Run(async () =>
                {
                    return await _youtubeService.GetBestAudioStreamAsync(item.Url);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
            finally
            {
                IsLoading = false;
            }

            if (streamInfo == null) return;

            Thumbnails = item.Thumbnails[0].Url;
            TitleSong = item.Title;
            Channel = item.Author.ChannelTitle;

            _mediaService.Play(streamInfo.Url);
        }


        public void UpdateProgress()
        {
            CurrentTime = _mediaService.CurrentTime;
            TotalTime = _mediaService.TotalTime;

            if (CurrentTime > TimeSpan.Zero && TotalTime > TimeSpan.Zero)
            {
                ProgressTime = CurrentTime.Value.TotalSeconds / TotalTime.Value.TotalSeconds;
            }
            else
            {
                ProgressTime = 0.0;
            }
        }
    }
}
