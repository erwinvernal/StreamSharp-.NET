using PruebaAPP.Objetos.Functions;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Views.Android.ViewModels;
using System.Diagnostics;

namespace PruebaAPP.Views.Android;

public partial class Android_View_SelectedPlaylist : ContentView
{
	public Android_View_SelectedPlaylist()
	{
		InitializeComponent();
	}


    private async void Click_SelectedItemsMenu(object sender, EventArgs e)
    {
        // Verificar si el sender es un bot�n
        if (sender is not Button btn)
            return;

        // Bloquear el bot�n para evitar m�ltiples clics
        btn.IsEnabled = false;

        // Asegurarse de reactivar el bot�n al final
        try
        {
            // Obtener la canci�n asociada al bot�n
            if (btn.BindingContext is not Song sng)
                return;

            // Verificar si el la canci�n es nulo o no tiene un ID v�lido
            if (sng is null || string.IsNullOrWhiteSpace(sng.Id)) 
                return;

            // Verificar si el BindingContext es del tipo esperado
            if (BindingContext is not MainViewModel vm)
                return;

            // Abrir el men� contextual
            var title  = "Selecciona acci�n";
            var cancel = "Cancelar";
            var param  = new[] { "Reproducir", "Quitar de la playlist" };
            var action = await DialogHelpers.DisplayAction(title, cancel, null, param);

            // Ejecutar la acci�n seleccionada
            switch (action)
            {
                // Reproducir canci�n
                case "Reproducir":
                    await vm.Play(sng.Id!);
                    break;

                // Quitar canci�n de la playlist
                case "Quitar de la playlist":

                    // Verificar si hay una playlist seleccionada
                    if (vm.Player.SelectedPlaylist?.Id is null) return;

                    // Quitar canci�n de la playlist
                    await vm.Playlist_ToDelete(new MainViewModel.PlaylistRemoveParam(vm.Player.SelectedPlaylist.Id, sng.Id));
                    break;
            }
        } finally
        {
            // Reactivar el bot�n
            btn.IsEnabled = true;
        }
    }

    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not CollectionView cv || BindingContext is not MainViewModel vm)
            return;

        if (vm.IsLoading2) return;

        if (e.CurrentSelection.Count == 0 || e.CurrentSelection[0] is not Song selected)
            return;


        cv.IsEnabled = false;

        try
        {
            // Obtener �ndice en la colecci�n
            if (cv.ItemsSource is IList<Song> items)
                vm.Player.CurrentSongIndex = items.IndexOf(selected);
            else
                vm.Player.CurrentSongIndex = 0;

            // Reproducir canci�n
            await vm.Play(selected.Id!);
        }
        finally
        {
            cv.IsEnabled = true;
        }
    }

    private void Click_BackPage(object sender, EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        vm.CurrentView = new Android_View_Playlist();
    }
}