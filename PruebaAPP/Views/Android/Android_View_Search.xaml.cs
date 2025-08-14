using CommunityToolkit.Mvvm.Input;
using PruebaAPP.Views.Android.ViewModels;
using System.Diagnostics;
using YoutubeExplode.Search;

namespace PruebaAPP.Views.Android
{
    public partial class Android_View_Search : ContentView
    {
        public Android_View_Search()
        {
            InitializeComponent();
            // No reasignar BindingContext aqu� para que herede del padre (si lo est�s usando dentro de la p�gina).
        }

        // Event handler cuando el CollectionView detecta que quedan pocos items
        private void ResultsCollection_RemainingItemsThresholdReached(object sender, EventArgs e)
        {
            // Intentamos invocar el comando del VM
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                try
                {
                    // Si usas CommunityToolkit, el comando generado es LoadMoreCommand
                    if (vm.LoadMoreCommand?.CanExecute(null) ?? false)
                    {
                        vm.LoadMoreCommand.Execute(null);
                    }
                    else
                    {
                        // Alternativa: invocar el m�todo directamente (si existe)
                        // _ = vm.LoadMore();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error executing LoadMoreCommand: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine("BindingContext no es AndroidPropertyViewModel (o no est� heredado).");
            }
        }

        // Tu handler de selecci�n ya existente
        private async void Click_SelectedItems(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is not VideoSearchResult selected) return;

            // Limpiar selecci�n visual
            if (sender is CollectionView cv) cv.SelectedItem = null;

            // Ejecuta el handler / comando del VM
            if (BindingContext is AndroidPropertyViewModel vm)
            {
                // Si el comando es IAsyncRelayCommand (caso normal con [RelayCommand] async Task ...)
                if (vm.ItemSelectionChangedCommand is IAsyncRelayCommand asyncCmd)
                {
                    await asyncCmd.ExecuteAsync(selected);
                    return;
                }

                // Si es ICommand normal (fallback)
                var cmd = vm.ItemSelectionChangedCommand;
                if (cmd != null && cmd.CanExecute(selected))
                {
                    cmd.Execute(selected);
                }
            }
        }
    }
}
