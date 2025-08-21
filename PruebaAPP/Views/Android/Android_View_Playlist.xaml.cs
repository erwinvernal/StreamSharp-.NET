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

    private void Click_SelectItemsPlay(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (BindingContext is MainViewModel vm)
            {
                if (e.CurrentSelection?.FirstOrDefault() is not Objetos.Models.Playlist selected)
                    return;

                // Limpiar selección visual
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;

                // Abrimos la playlist seleccionada
                vm.Playlist_View(selected);
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al reproducir favorito: {ex.Message}");
        }
    }

    private async void Click_SelectedItemsMenu(object sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
        {
            if (sender is Button btn && btn.BindingContext is Playlist fav)
            {

                // Verificar si el favorito es nulo o no tiene un ID válido
                if (fav is null || string.IsNullOrWhiteSpace(fav.Id)) return;

                // Ejecutamos accion
                await vm.Playlist_Delete(fav.Id);

            }
        }
    }

}