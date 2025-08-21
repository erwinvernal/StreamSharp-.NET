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
        if (BindingContext is MainViewModel vm)
        {
            await vm.Playlist_Play(vm.Player.ViewPlaylist);
        }
    }

    private async void Click_SelectView(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (BindingContext is MainViewModel vm)
            {
                if (e.CurrentSelection?.FirstOrDefault() is not Objetos.Models.Song selected)
                    return; // Salimos si no hay selecci�n

                // Limpiar selecci�n visual
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;

                // Obtener el �ndice del item seleccionado
                int selectedIndex = vm.Player.ViewPlaylist.Items.IndexOf(selected);

                // Guardamos �ndice y playlist
                vm.Player.CurrentSongIndex = selectedIndex;
                vm.Player.SelectedPlaylist = vm.Player.ViewPlaylist;

                // Asignamos a la playlist que se est� visualizando
                foreach (var item in vm.Playlists)
                {
                    if (item.Id != vm.Player.SelectedPlaylist.Id)
                    {
                        item.IsPlaying = false;
                    }
                }
                vm.Player.SelectedPlaylist.IsPlaying = true;


                // Limpeamos viewplaylist
                vm.Player.ViewPlaylist = new Objetos.Models.Playlist();

                // Abrimos vista
                vm.CurrentView = new Android_View_SelectedPlaylist();

                // Reproducimos
                await vm.PlaySongById(selected.Id!);

            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al reproducir favorito: {ex.Message}");
        }

    }

    private async void Click_DeleteItems(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Objetos.Models.Song fav)
        {
            // Obtener la p�gina actual de forma segura usando la ventana asociada
            var page = this.Window?.Page;
            if (page == null)
            {
                Debug.WriteLine("No se pudo obtener la p�gina actual para mostrar el men� contextual.");
                return;
            }

            // Verificar si el favorito es nulo o no tiene un ID v�lido
            if (fav is null || string.IsNullOrWhiteSpace(fav.Id)) return;

            // Abrir el men� contextual
            string title = "Selecciona acci�n";
            string cancel = "Cancelar";
            string[] param = { "Reproducir", "Quitar de la playlist", "Almacenar en memoria" };
            string action = await page.DisplayActionSheet(title, cancel, null, param);

            // Ejecutar la acci�n seleccionada
            if (BindingContext is MainViewModel vm)
            {
                switch (action)
                {
                    case "Reproducir":
                        await vm.PlaySongById(fav.Id!);
                        break;

                    case "Quitar de la playlist":
                        if (vm.Player.SelectedPlaylist?.Id is null) return;

                        await vm.Playlist_ToDelete(new MainViewModel.PlaylistRemoveParam(vm.Player.SelectedPlaylist.Id, fav.Id));
                        break;

                    case "Almacenar en memoria":
                        break;
                }
            }
        }
    }

}