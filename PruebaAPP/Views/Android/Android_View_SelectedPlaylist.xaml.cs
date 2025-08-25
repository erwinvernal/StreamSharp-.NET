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
        // Verificar si el sender es un botón
        if (sender is not Button btn)
            return;

        // Bloquear el botón para evitar múltiples clics
        btn.IsEnabled = false;

        // Asegurarse de reactivar el botón al final
        try
        {
            // Obtener la canción asociada al botón
            if (btn.BindingContext is not Song sng)
                return;

            // Verificar si el la canción es nulo o no tiene un ID válido
            if (sng is null || string.IsNullOrWhiteSpace(sng.Id)) 
                return;

            // Verificar si el BindingContext es del tipo esperado
            if (BindingContext is not MainViewModel vm)
                return;

            // Abrir el menú contextual
            var title  = "Selecciona acción";
            var cancel = "Cancelar";
            var param  = new[] { "Reproducir", "Quitar de la playlist" };
            var action = await DialogHelpers.DisplayAction(title, cancel, null, param);

            // Ejecutar la acción seleccionada
            switch (action)
            {
                // Reproducir canción
                case "Reproducir":
                    await vm.Play(sng.Id!);
                    break;

                // Quitar canción de la playlist
                case "Quitar de la playlist":

                    // Verificar si hay una playlist seleccionada
                    if (vm.Player.SelectedPlaylist?.Id is null) return;

                    // Quitar canción de la playlist
                    await vm.Playlist_ToDelete(new MainViewModel.PlaylistRemoveParam(vm.Player.SelectedPlaylist.Id, sng.Id));
                    break;
            }
        } finally
        {
            // Reactivar el botón
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
            // Obtener índice en la colección
            if (cv.ItemsSource is IList<Song> items)
                vm.Player.CurrentSongIndex = items.IndexOf(selected);
            else
                vm.Player.CurrentSongIndex = 0;

            // Reproducir canción
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