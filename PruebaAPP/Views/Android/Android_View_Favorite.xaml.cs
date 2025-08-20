using PruebaAPP.Views.Android.ViewModels;
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
        try
        {
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                if (e.CurrentSelection?.FirstOrDefault() is not Objetos.Models.Song selected)
                    return;

                // Limpiar selección visual
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;

                if (!string.IsNullOrWhiteSpace(selected.Id))
                {
                    await vm.PlaySongById(selected.Id);
                }

                OnPropertyChanged(nameof(selected.Id));
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al reproducir favorito: {ex.Message}");
        }
    }
    private async void Click_SelectedItemsMenu(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is Objetos.Models.Song fav)
        {
            // Obtener la página actual de forma segura usando la ventana asociada
            var page = this.Window?.Page;
            if (page == null)
            {
                Debug.WriteLine("No se pudo obtener la página actual para mostrar el menú contextual.");
                return;
            }

            // Verificar si el favorito es nulo o no tiene un ID válido
            if (fav is null || string.IsNullOrWhiteSpace(fav.Id)) return;

            // Abrir el menú contextual
            string   title  = "Selecciona acción";
            string   cancel = "Cancelar";
            string[] param  = { "Reproducir", "Quitar de favoritos", "Agregar a una playlist", "Almacenar en memoria" };
            string   action = await page.DisplayActionSheet(title, cancel, null, param);

            // Ejecutar la acción seleccionada
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                switch (action)
                {
                    case "Reproducir":
                        await vm.PlaySongById(fav.Id!);
                        break;

                    case "Quitar de favoritos":
                        await vm.Favorite_Delete(fav.Id!);
                        break;

                    case "Agregar a una playlist":
                        await vm.Playlist_ToAdd(fav);
                        break;

                    case "Almacenar en memoria":
                        break;
                }
            }
        }
    }
    private async void Click_PlayAll(object sender, EventArgs e)
    {
        if (BindingContext is AndroidPropertyViewModel vm)
        {
        
            // Creamos una playlist temporal con los favoritos
            var tempPlaylist = new Objetos.Models.Playlist
            {
                Id = "temp_favorites",
                Title = "Mis Favoritos",
                Items = vm.Favoritos
            };
        
            // Asignamos la playlist temporal a la propiedad SelectedPlaylist
            vm.SelectedPlaylist = tempPlaylist;
        
            // Limpiamos el índice actual de la canción
            vm.CurrentSongIndex = 0;
        
            // Reproducimos la playlist temporal
            if (tempPlaylist.Items.Count > 0)
            {
                // Reproducir la primera canción de la lista
                var firstSong = tempPlaylist.Items.FirstOrDefault();
                if (firstSong != null && !string.IsNullOrWhiteSpace(firstSong.Id))
                {
                    await vm.PlaySongById(firstSong.Id);
                }
            }
            else
            {
                Debug.WriteLine("No hay canciones en la lista de favoritos para reproducir.");
            }
        }
        
    }
}