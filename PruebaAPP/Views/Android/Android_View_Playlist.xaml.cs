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
        // Aseguramos que el sender es un CollectionView
        if (sender is not CollectionView cv)
            return;

        // Deshabilitamos la selección y limpiamos la selección actual
        cv.IsEnabled = false;
        cv.SelectedItem = null;

        // Aseguramos reactivar la selección al final
        try
        {
            // Obtenemos el viewmodel
            if (BindingContext is not MainViewModel vm)
                return;

            // Obtenemos el item seleccionado
            if (e.CurrentSelection.Count == 0 || e.CurrentSelection[0] is not Playlist selected)
                return;

            // Abrimos la playlist seleccionada
            vm.Playlist_View(selected);
        }
        finally
        {
            cv.IsEnabled = true;
        }
    }
    private async void Click_SelectedItemsMenu(object sender, EventArgs e)
    {
        // Aseguramos que el sender es un boton
        if (sender is not Button btn)
            return;

        // Bloqueamos interface
        btn.IsEnabled = false;

        // Aseguramos reactivar la selección al final
        try
        {
            // Obtenemos el viewmodel
            if (BindingContext is not MainViewModel vm)
                return;

            // Obtenemos el playlist
            if (btn.BindingContext is not Playlist pl)
            return;

            // Verificar si el favorito es nulo o no tiene un ID válido
            if (pl is null || string.IsNullOrWhiteSpace(pl.Id)) return;

            // Ejecutamos accion
            await vm.Playlist_Delete(pl.Id);
        }
        finally
        {
            btn.IsEnabled = true;
        }
       
    }

}