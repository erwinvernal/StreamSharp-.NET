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
                if (e.CurrentSelection?.FirstOrDefault() is not Objetos.Models.Favorite selected)
                    return;

                // Limpiar selecci�n visual
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
        if (sender is Button btn && btn.BindingContext is Objetos.Models.Favorite fav)
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
            string[] param = { "Reproducir", "Eliminar" };
            string action = await page.DisplayActionSheet(title, cancel, null, param);

            // Ejecutar la acci�n seleccionada
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                switch (action)
                {
                    case "Reproducir":
                        await vm.PlaySongById(fav.Id!);
                        break;

                    case "Eliminar":
                        await vm.DeleteFavorite(fav.Id!);
                        break;
                }
            }
        }
    }
}