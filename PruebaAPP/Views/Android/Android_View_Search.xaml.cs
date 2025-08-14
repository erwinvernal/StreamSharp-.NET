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
            // No reasignar BindingContext aquí para que herede del padre (si lo estás usando dentro de la página).
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
                        // Alternativa: invocar el método directamente (si existe)
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
                Debug.WriteLine("BindingContext no es AndroidPropertyViewModel (o no está heredado).");
            }
        }

        // Tu handler de selección ya existente
        private async void Click_SelectedItems(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is not VideoSearchResult selected) return;

            // Limpiar selección visual
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
