using PruebaAPP.Objetos.Models;
using PruebaAPP.Views.Android.ViewModels;
using System.Diagnostics;
using static PruebaAPP.Views.Android.ViewModels.AndroidPropertyViewModel;

namespace PruebaAPP.Views.Android;

public partial class Android_View_Playlist : ContentView
{
	public Android_View_Playlist()
	{
		InitializeComponent();
	}

    private void Click_Delete(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Objetos.Models.Song fav)
        {
            if (BindingContext is AndroidPropertyViewModel vm && vm.SelectedPlaylist != null && fav.Id != null)
            {
                // Usamos la playlist seleccionada y el favorite del botón
                var param = new PlaylistRemoveParam(vm.SelectedPlaylist.Id, fav.Id);

                _ = vm.Playlist_ToDelete(param);
            }
        }
    }

    private async void Click_PlayPlaylist(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                if (e.CurrentSelection?.FirstOrDefault() is not Song selected)
                    return;

                // Limpiar selección visual
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;

                // Asegurarse de que Items no es null antes de acceder a él
                var items = vm.SelectedPlaylist?.Items;
                if (items == null)
                    return;

                int index = items.IndexOf(selected);
                if (index >= 0 && !string.IsNullOrWhiteSpace(selected.Id))
                {
                    vm.CurrentSongIndex = index;
                    await vm.PlaySongById(selected.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al reproducir favorito: {ex.Message}");
        }
    }

    private void Click_SelectedPlaylist(object sender, SelectionChangedEventArgs e)
    {

    }

    private async void Click_DeletePlayList(object sender, EventArgs e)
    {
        try
        {
            // Obtenemos la playlist asociada al botón
            if (sender is Button btn && btn.BindingContext is Playlist playlist)
            {
                if (BindingContext is AndroidPropertyViewModel vm)
                {
                    // Borramos playlist
                    await vm.Playlist_Delete(playlist.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al eliminar playlist: {ex.Message}");
        }
    }

    private void Click_Menu(object sender, EventArgs e)
    {

    }
}