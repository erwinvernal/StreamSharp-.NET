using CommunityToolkit.Maui.Core;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Objetos.Services;
using PruebaAPP.Views.Android.ViewModels;
using YoutubeExplode.Common;
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
                _vm.CurrentView = new Android_View_Search
                {
                    BindingContext = _vm
                };

            // Timer para actualizar progreso
                _refreshTimer = new System.Timers.Timer(500);
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

        private async void Click_AddFavoriteSong(object sender, EventArgs e)
        {
            if (_vm.CurrentSong is not null && !string.IsNullOrEmpty(_vm.CurrentSong.Id))
            {
                if (_vm.CurrentSong.IsFavorite == true)
                {
                    // Eliminamos
                    await _vm.Favorite_Delete(_vm.CurrentSong.Id!);

                    // Cambiamos icono
                    _vm.CurrentSong.IsFavorite = false;
                }
                else
                {
                    // Agregamos
                    var favorito = new Song
                    {
                        Id         = _vm.CurrentSong.Id,
                        Title      = _vm.CurrentSong.Title,
                        Author     = _vm.CurrentSong.Author,
                        Thumbnails = _vm.CurrentSong.Thumbnails,
                        Duration   = _vm.CurrentSong.Duration,
                        IsFavorite = true

                    };

                    // Ejecutar el comando
                    if (_vm.Favorite_AddCommand.CanExecute(favorito))
                        _vm.Favorite_AddCommand.Execute(favorito);

                    // Cambiamos icono
                    _vm.CurrentSong.IsFavorite = true;

                    // Agregamos a favoritos
                    await _vm.Playlist_ToAdd(favorito);

                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _refreshTimer?.Stop();
            _refreshTimer?.Dispose();
        }

        // Seleccionamos view 
            private void Click_OpenView(object sender, EventArgs e)
            {

                // Cambiamos el color de todos los controles a blanco
                foreach (var ctrl in BarButttonContainer.Children)
                {
                    if (ctrl is Button btn)
                    {
                        if (Application.Current?.Resources?.TryGetValue("foreg_night_2", out var colorObj) ?? false)
                        {
                            btn.TextColor = (Color)colorObj;
                        }
                    }
                }

                if (sender is Button rb && int.TryParse(rb.ClassId, out int tipo))
                {

                    // establecemos color al botón seleccionado
                    if (Application.Current?.Resources?.TryGetValue("foreg_night_0", out var colorObj) ?? false)
                    {
                        rb.TextColor = (Color)colorObj;
                    }

                    // Seleccionamos view y la asignamos a ModuleContainer.Content directamente
                    switch (tipo)
                    {
                        case 0: _vm.CurrentView = new Android_View_Control(); break;
                        case 1: _vm.CurrentView = new Android_View_Search(); break;
                        case 2: _vm.CurrentView = new Android_View_Playlist(); break;
                        case 3: _vm.CurrentView = new Android_View_Favorite(); break;
                        default: break;
                    }
                }
        }

        // Eventos del reproductor
            private void MediaOpen_PlayerM(object sender, EventArgs e)
            {
                //PlayerM.IsVisible = true;
                //_refreshTimer.Start();
            }

            private void StateChanged_PlayerM(object sender, MediaStateChangedEventArgs e)
            {
                _vm.CurrentMediaState = PlayerM.CurrentState;
            }

        private void MediaEnded_PlayerM(object sender, EventArgs e)
        {
            // Detener y limpiar antes de reproducir siguiente
            _vm.MediaP_Stop();
            _ = _vm.MediaP_SkipNext();
        }
    }
}
