using PruebaAPP.Objetos.Models;
using PruebaAPP.Views.Android.ViewModels;
using System.Diagnostics;
using YoutubeExplode.Search;

namespace PruebaAPP.Views.Android;

public partial class Android_View_Favorite : ContentView
{
	public Android_View_Favorite()
	{
		InitializeComponent();
        rb0.IsChecked = true; // Seleccionar por defecto
    }

    private async void Click_SelectItems(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                if (e.CurrentSelection?.FirstOrDefault() is not Favorite selected)
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
        if (sender is Button btn && btn.BindingContext is Favorite fav)
        {
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                if (!string.IsNullOrWhiteSpace(fav.Id))
                {
                    vm.DeleteFavorite(fav.Id);
                }
            }
        }
    }

    private void Click_Changetype(object sender, CheckedChangedEventArgs e)
    {
        if (BindingContext is AndroidPropertyViewModel vm && e.Value) // Solo cuando se selecciona
        {
            if (sender is RadioButton rb && int.TryParse(rb.ClassId, out int tipo))
            {
                switch (tipo)
                {
                    case 0: vm.ChangeFavoriteType(FType.Cancion); break;
                    case 1: vm.ChangeFavoriteType(FType.Playlist); break;
                    case 2: vm.ChangeFavoriteType(FType.Canal); break;
                    default: break;
                }
            }
        }
    }
}