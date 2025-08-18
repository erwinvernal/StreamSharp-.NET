using PruebaAPP.Views.Android.ViewModels;
using System.Diagnostics;
using YoutubeExplode.Playlists;

namespace PruebaAPP.Views.Android.View_Favorite;

public partial class Android_Favorite_Playlist : ContentView
{
	public Android_Favorite_Playlist()
	{
		InitializeComponent();
	}

    private void Click_SelectedPlaylist(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                if (e.CurrentSelection?.FirstOrDefault() is not Objetos.Models.Playlist selected)
                    return;

                // Limpiar selección visual
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;

                if (selected is not null)
                {
                    vm.SelectedPlaylist = selected;

                    // Cambiamos el ContentView
                    vm.CurrentView = new Android_View_Playlist();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al reproducir favorito: {ex.Message}");
        }
    }

    private async void Click_DeletePlayList(object sender, EventArgs e)
    {
        try
        {
            if (BindingContext is AndroidPropertyViewModel vm)
            {
            }
        } catch (Exception ex) {
            Debug.WriteLine($"Error al eliminar favorito: {ex.Message}");
        }
    }
}