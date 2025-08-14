using PruebaAPP.Views.Android.Services;
using PruebaAPP.Views.Android.ViewModels;
using YoutubeExplode.Search;

namespace PruebaAPP.Views.Android
{
    public partial class Android_Page_Main : ContentPage
    {
        private readonly AndroidPropertyViewModel _vm;
        private readonly System.Timers.Timer _refreshTimer;

        public Android_Page_Main()
        {
            InitializeComponent();

            // Servicios
                var mediaService = new MediaPlayerService(PlayerM);
                var youtubeService = new YouTubeService();

            // ViewModel
                _vm = new AndroidPropertyViewModel(youtubeService, mediaService, Dispatcher);
                BindingContext = _vm;

            // Asignar el ContentView del Search
                ModuleContainer.Content = new Android_View_Search
                {
                    BindingContext = _vm
                };

            // Timer para actualizar progreso
                _refreshTimer = new System.Timers.Timer(1000);
                _refreshTimer.Elapsed += (s, e) =>
                {
                    MainThread.BeginInvokeOnMainThread(() => _vm.UpdateProgress());
                };
                _refreshTimer.Start();
        }

        // Cuando el usuario selecciona un item en el CollectionView
        private async void ResultsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = e.CurrentSelection?.FirstOrDefault() as VideoSearchResult;
            if (selected == null) return;

            // Limpiar selección visual
            if (sender is CollectionView cv) cv.SelectedItem = null;

            await _vm.ItemSelectionChanged(selected);
        }

        private void MediaOpen_PlayerM(object sender, EventArgs e)
        {
            //PlayerM.IsVisible = true;
        }

        private void MediaEnded_PlayerM(object sender, EventArgs e)
        {
            //PlayerM.IsVisible = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }
    }
}
