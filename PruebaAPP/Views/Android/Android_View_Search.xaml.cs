using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using YoutubeExplode.Search;

namespace PruebaAPP.Views.Android;

public partial class Android_View_Search : ContentView
{
    // Propiedades de la clase
        public Android_Property? vm { get; set; }

    // Inicializacion
	    public Android_View_Search(Android_Property vm)
	    {
            // Inicializacion
		        InitializeComponent();
                this.vm = vm;
                this.BindingContext = this.vm;

            // Enlazamos 
                this.Loaded += Android_View_Search_Loaded;
	    }
        private async void Android_View_Search_Loaded(object? sender, EventArgs e)
        {
            await vm!.SearchInternal("Musica");
            
        }

        private async void Click_SelectedItems(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
                return;

            var item = e.CurrentSelection[0] as VideoSearchResult;
            if (item == null)
                return;

            await vm!.ItemSelectionChanged(item);

            // Espera un frame para que MAUI procese el cambio de selección
            await Task.Yield(); // o Task.Delay(1);
            
            ((CollectionView)sender).SelectedItem = null;

        }

}