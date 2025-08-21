using PruebaAPP.Objetos.Models;
using PruebaAPP.Views.Android.ViewModels;
using System.Diagnostics;

namespace PruebaAPP.Views.Android;

public partial class Android_View_Playlist : ContentView
{
	public Android_View_Playlist()
	{
		InitializeComponent();
	}

    private async void Click_DeletePlayList(object sender, EventArgs e)
    {
        try
        {
            // Obtenemos la playlist asociada al botón
            if (sender is Button btn && btn.BindingContext is Playlist playlist)
            {
                if (BindingContext is MainViewModel vm)
                {

                    // Limpiar selección visual
                    if (sender is CollectionView cv)
                        cv.SelectedItem = null;

                    // Borramos playlist
                    if(playlist.Id is not null)
                        await vm.Playlist_Delete(playlist.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al eliminar playlist: {ex.Message}");
        }
    }

    private void Click_SelectedPlaylist(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (BindingContext is MainViewModel vm)
            {
                if (e.CurrentSelection?.FirstOrDefault() is not Playlist selected)
                    return;

                // Limpiar selección visual
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;

                if (!string.IsNullOrWhiteSpace(selected.Id))
                {
                    //await vm.Playlist(selected.Id);
                }

                OnPropertyChanged(nameof(selected.Id));
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al reproducir favorito: {ex.Message}");
        }
    }
}