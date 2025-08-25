using PruebaAPP.Views.Android.ViewModels;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Objetos.Functions;
using System.Diagnostics;

namespace PruebaAPP.Views.Android;
public partial class Android_View_Favorite : ContentView
{
	public Android_View_Favorite()
	{
		InitializeComponent();
    }

    private async void Click_SelectItemsPlay(object sender, SelectionChangedEventArgs e)
    {
        // Verificar si el sender es un bot�n
        if (sender is not CollectionView cv) 
            return;

        // Aplicamos acciones al control
        cv.IsEnabled    = false;                // Deshabilitar interacci�n
        cv.SelectedItem = null;                 // Limpiar selecci�n visual

        // Operaci�n de selecci�n de canci�n
        try
        {
            // Verificar si el BindingContext es del tipo esperado
            if (BindingContext is not MainViewModel vm)
                return;

            // Verificar si hay una selecci�n actual y obtener el primer elemento
            if (e.CurrentSelection.Count == 0 || e.CurrentSelection[0] is not Objetos.Models.Song sng)
                return;

            // Verificar si el elemento seleccionado es nulo o no tiene un ID v�lido
            if (string.IsNullOrWhiteSpace(sng.Id))
                return;

            // Reproducimos la canci�n seleccionada
            await vm.Play(sng.Id);

            // Notificar que se ha cambiado la canci�n actual
            OnPropertyChanged(nameof(sng.Id));

        }
        catch (Exception ex)
        {
            var title   = "Error al reproducir";
            var message = $"No se pudo reproducir la canci�n seleccionada. Intentelo de nuevo.";
            var ok      = "Aceptar";
            Debug.WriteLine($"Error al reproducir favorito: {ex.Message}");
            await DialogHelpers.DisplayMessage(title, message, ok);
        } finally
        {
            // Habilitar interacci�n
            cv.IsEnabled = true;
        }
    }
    private async void Click_SelectedItemsMenu(object sender, EventArgs e)
    {
        // Verificar si el sender es un bot�n
        if (sender is not Button btn) return;

        // Deshabilitar el bot�n mientras se procesa
        btn.IsEnabled = false;

        // Operaci�n del men� contextual
        try
        {
            // Verificar si el bot�n y su BindingContext son del tipo esperado
            if (btn.BindingContext is not Song sng)
                return;

            // Verificar si el favorito es nulo o no tiene un ID v�lido
            if (sng is null || string.IsNullOrWhiteSpace(sng.Id)) 
                return;

            // Verificar si el BindingContext es del tipo esperado
            if (BindingContext is not MainViewModel vm)
                return;

            // Abrir el men� contextual
            var title  = "Selecciona acci�n";
            var cancel = "Cancelar";
            var param  = new[] { "Reproducir", "Quitar de favoritos", "Agregar a una playlist"};
            var action = await DialogHelpers.DisplayAction(title, cancel, null, param);

            // Verificamos la acci�n seleccionada
            switch (action)
            {
                case "Reproducir":
                    await vm.Play(sng.Id!);
                    break;

                case "Quitar de favoritos":
                    await vm.Favorite_Delete(sng.Id!);
                    break;

                case "Agregar a una playlist":
                    await vm.Playlist_ToAdd(sng);
                    break;
            }

        } finally
        {
            // Habilitar el bot�n nuevamente
            btn.IsEnabled = true;
        }
    }
    private async void Click_PlayAll(object sender, EventArgs e)
    {
        // Verificar si el sender es un bot�n
        if (sender is not Button btn) return;

        // Deshabilitar el bot�n mientras se procesa
        btn.IsEnabled = false;

        // Verificar si el BindingContext es del tipo esperado
        if (BindingContext is not MainViewModel vm)
            return;

        // Operaci�n de reproducci�n de todos los favoritos
        try
        {
            // Creamos una playlist temporal con los favoritos
            var tempPlaylist = new Playlist
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Mis Favoritos",
                Items = vm.Favoritos
            };

            // Asignamos la playlist temporal a la propiedad SelectedPlaylist
            vm.Player.SelectedPlaylist = tempPlaylist;

            // Limpiamos el �ndice actual de la canci�n
            vm.Player.CurrentSongIndex = 0;

            // Reproducimos la playlist temporal
            if (tempPlaylist.Items.Count > 0)
            {
                // Reproducir la primera canci�n de la lista
                var firstSong = tempPlaylist.Items.FirstOrDefault();
                if (firstSong != null && !string.IsNullOrWhiteSpace(firstSong.Id))
                {
                    await vm.Play(firstSong.Id);
                }

                // Cambiamos de ventana
                //vm.CurrentView = new Android_View_CurrentPlaylist();
            }
            else
            {
                var title = "Lista vac�a";
                var message = "No hay canciones en la lista de favoritos para reproducir.";
                var ok = "Aceptar";
                await DialogHelpers.DisplayMessage(title, message, ok);
            }
        }
        finally
        {
            // Habilitar el bot�n nuevamente
            btn.IsEnabled = true;
        }
    }
}