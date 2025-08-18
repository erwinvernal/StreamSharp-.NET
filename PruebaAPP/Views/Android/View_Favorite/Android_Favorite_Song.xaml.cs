using PruebaAPP.Views.Android.ViewModels;
using PruebaAPP.Objetos.Models;
using System.Diagnostics;

namespace PruebaAPP.Views.Android.View_Favorite;

public partial class Android_Favorite_Song : ContentView
{
    public Android_Favorite_Song()
    {
        InitializeComponent();
    }

    private async void Click_SelectItems(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                if (e.CurrentSelection?.FirstOrDefault() is not Objetos.Models.Favorite selected)
                    return;

                // Limpiar selección visual
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;

                if (!string.IsNullOrWhiteSpace(selected.Id))
                {
                    await vm.PlaySongById(selected.Id);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al reproducir favorito: {ex.Message}");
        }
    }

    private void Click_Delete(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Objetos.Models.Favorite fav)
        {
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                if (!string.IsNullOrWhiteSpace(fav.Id))
                {
                    _ = vm.DeleteFavorite(fav.Id);
                }
            }
        }
    }

    private async void Click_AddToPlaylist(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Objetos.Models.Favorite fav)
        {
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                
                await vm.Playlist_ToAdd(fav);
            }
        }
    }
}