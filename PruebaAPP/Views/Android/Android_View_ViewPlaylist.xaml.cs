using PruebaAPP.Objetos.Functions;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Views.Android.ViewModels;
using System.Diagnostics;

namespace PruebaAPP.Views.Android;

public partial class Android_View_ViewPlaylist : ContentView
{
	public Android_View_ViewPlaylist()
	{
		InitializeComponent();
    }

    private async void Click_PlayAll(object sender, EventArgs e)
    {
        // Verificar si el BindingContext es del tipo esperado
        if (BindingContext is not MainViewModel vm)
            return;

        // Cargamos lista
        await vm.Playlist_Play(vm.Player.ViewPlaylist);
    }

    private async void Click_SelectView(object sender, SelectionChangedEventArgs e)
    {

        // Verificar si hay una selección actual y obtener el primer elemento
        if (e.CurrentSelection.Count == 0 || e.CurrentSelection[0] is not Song selected)
            return;

        // Verificamos si se esta cargando
        if (BindingContext is not MainViewModel vm)
            return;

        // Limpiar selección visual
        if (sender is CollectionView cv)
            cv.SelectedItem = null;

        // Obtener el índice del item seleccionado
        int selectedIndex = vm.Player.ViewPlaylist.Items.IndexOf(selected);

        // Asignamos la playlist seleccionada
        await vm.Playlist_Play(vm.Player.ViewPlaylist, selected.Id!, selectedIndex);
        vm.Player.SelectedPlaylist = vm.Player.ViewPlaylist;

    }

    private async void Click_DeleteItems(object sender, EventArgs e)
    {
        // Verificar si el elemento sender es un botón
        if (sender is not Button btn)
            return;

        // Verificar si el BindingContext del botón es del tipo esperado
        if (btn.BindingContext is not Song fav)
            return;

        // Verificar si el BindingContext es del tipo esperado
        if (BindingContext is not MainViewModel vm)
            return;

        // Verificar si el favorito es nulo o no tiene un ID válido
        if (fav is null || string.IsNullOrWhiteSpace(fav.Id)) return;

        // Abrir el menú contextual
        var title = "Selecciona acción";
        var cancel = "Cancelar";
        var param = new[] { "Reproducir", "Quitar de la playlist" };
        var action = await DialogHelpers.DisplayAction(title, cancel, null, param);

        // Ejecutar la acción seleccionada
        switch (action)
        {
            case "Reproducir":
                await vm.Play(fav.Id!);
                break;

            case "Quitar de la playlist":

                // Verificamos que la playlist este asignada
                if (vm.Player.ViewPlaylist.Id is null) 
                    return;

                // Procedemos a eliminar
                await vm.Playlist_ToDelete(new MainViewModel.PlaylistRemoveParam(vm.Player.ViewPlaylist.Id, fav.Id));
                break;

        }
    }

    private void Click_BackPage(object sender, EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        vm.CurrentView = new Android_View_Playlist();
    }
}