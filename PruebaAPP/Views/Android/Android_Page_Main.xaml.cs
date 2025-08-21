using CommunityToolkit.Maui.Core;
using PruebaAPP.Objetos.Models;
using PruebaAPP.ViewModels;
using PruebaAPP.Views.Android.ViewModels;
using YoutubeExplode.Search;

namespace PruebaAPP.Views.Android
{
    public partial class Android_Page_Main : ContentPage
    {
        private readonly MainViewModel _vm;

        public Android_Page_Main()
        {
            InitializeComponent();

            // ViewModel
            _vm = new MainViewModel(new PlayerViewModel(PlayerM), Dispatcher);
            BindingContext = _vm;

            // Asignar el ContentView del Search
            _vm.CurrentView = new Android_View_Search
            {
                BindingContext = _vm
            };
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
            if (_vm.Player.CurrentSong is not null && !string.IsNullOrEmpty(_vm.Player.CurrentSong.Id))
            {
                if (_vm.Player.CurrentSong.IsFavorite == true)
                {
                    // Eliminamos
                    await _vm.Favorite_Delete(_vm.Player.CurrentSong.Id!);

                    // Cambiamos icono
                    _vm.Player.CurrentSong.IsFavorite = false;
                }
                else
                {
                    // Agregamos
                    var favorito = new Song
                    {
                        Id         = _vm.Player.CurrentSong.Id,
                        Title      = _vm.Player.CurrentSong.Title,
                        Author     = _vm.Player.CurrentSong.Author,
                        Thumbnails = _vm.Player.CurrentSong.Thumbnails,
                        Duration   = _vm.Player.CurrentSong.Duration,
                        IsFavorite = true

                    };

                    // Ejecutar el comando
                    if (_vm.Favorite_AddCommand.CanExecute(favorito))
                        _vm.Favorite_AddCommand.Execute(favorito);

                    // Cambiamos icono
                    _vm.Player.CurrentSong.IsFavorite = true;

                    // Agregamos a favoritos
                    await _vm.Playlist_ToAdd(favorito);

                }
            }
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


        // Controlador del Slider
        private void Slider_DragStarted(object sender, EventArgs e)
        {
            PlayerM.Pause();
        }
        private void Slider_DragCompleted(object sender, EventArgs e)
        {
            Slider ctrl = (Slider)sender;
            PlayerM.SeekTo(TimeSpan.FromSeconds(ctrl.Value));
            PlayerM.Play();
        }

    }
}
