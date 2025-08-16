using PruebaAPP.Objetos.Models;
using PruebaAPP.Objetos.Services;
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

            // Inicializacion del servicio de media
                var mediaService = new MediaPlayerService(PlayerM);

            // ViewModel
                _vm = new AndroidPropertyViewModel(mediaService, Dispatcher);
                BindingContext = _vm;

            // Asignar el ContentView del Search
                ModuleContainer.Content = new Android_View_Search
                {
                    BindingContext = _vm
                };

            // Suscribir al evento
                _vm.ShowMessage += async (titulo, mensaje, botonAceptar, botonCancelar) =>
                {
                    if (string.IsNullOrEmpty(botonCancelar))
                        await this.DisplayAlert(titulo, mensaje, botonAceptar);
                    else
                        await this.DisplayAlert(titulo, mensaje, botonAceptar, botonCancelar);
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

            await _vm.PlaySong(selected);
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

        private void Click_OpenChannel(object sender, EventArgs e)
        {
            ModuleContainer.Content = new Android_View_Channel();
        }

        private void Click_OpenSearch(object sender, EventArgs e)
        {
            ModuleContainer.Content = new Android_View_Search();
        }
        private void Click_OpenFavorite(object sender, EventArgs e)
        {
            ModuleContainer.Content = new Android_View_Favorite();
        }

        private void Click_AddFavoriteSong(object sender, EventArgs e)
        {
            if (_vm.CurrentSong is not null && !string.IsNullOrEmpty(_vm.CurrentSong.Id))
            {
                if (_vm.CurrentSong.Favorite == true)
                {
                    // Eliminamos
                    _vm.DeleteFavorite(_vm.CurrentSong.Id!);

                    // Cambiamos icono
                    _vm.CurrentSong.Favorite = false;
                }
                else
                {
                    // Agregamos
                    var favorito = new Favorite
                    {
                        Id = _vm.CurrentSong.Id,
                        Title = _vm.CurrentSong.Title,
                        Author = _vm.CurrentSong.Channel,
                        Thumbnails = _vm.CurrentSong.Thumbnails,
                        Duracion = _vm.CurrentSong.Duration.ToString(),
                        Type = FType.Cancion
                    };

                    // Ejecutar el comando
                    if (_vm.AddFavoriteCommand.CanExecute(favorito))
                        _vm.AddFavoriteCommand.Execute(favorito);

                    // Cambiamos icono
                    _vm.CurrentSong.Favorite = true;
                }
            }
        }

    }
}
