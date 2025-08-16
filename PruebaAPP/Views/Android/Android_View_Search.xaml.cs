using CommunityToolkit.Mvvm.Input;
using PruebaAPP.Views.Android.ViewModels;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using YoutubeExplode.Search;

namespace PruebaAPP.Views.Android
{
    public partial class Android_View_Search : ContentView
    {
        // Declaracion de variables
            private static readonly HttpClient _http = new();
            private CancellationTokenSource? _cts;

        // Inicializacion del contructor
            public Android_View_Search()
            {
                InitializeComponent();
                // No reasignar BindingContext aquí se hereda del padre.
            }

        // Event handler cuando el CollectionView detecta que quedan pocos items
            private void ResultsCollection_RemainingItemsThresholdReached(object sender, EventArgs e)
            {
                // Intentamos invocar el comando del VM
                if (BindingContext is AndroidPropertyViewModel vm)
                {
                    try
                    {
                        _ = vm.LoadMore();
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

        // Seleccionar items
            private async void Click_SelectedItems(object sender, SelectionChangedEventArgs e)
            {
                if (e.CurrentSelection?.FirstOrDefault() is not VideoSearchResult selected) return;

                // Limpiar selección visual
                if (sender is CollectionView cv) cv.SelectedItem = null;

                // Ejecuta el handler / comando del VM
                if (BindingContext is AndroidPropertyViewModel vm)
                {
                    // Si el comando es IAsyncRelayCommand (caso normal con [RelayCommand] async Task ...)
                    if (vm.PlaySongCommand is IAsyncRelayCommand asyncCmd)
                    {
                        await asyncCmd.ExecuteAsync(selected);
                        return;
                    }

                    // Si es ICommand normal (fallback)
                    var cmd = vm.PlaySongCommand;
                    if (cmd != null && cmd.CanExecute(selected))
                    {
                        cmd.Execute(selected);
                    }
                }
            }

        // Busqueda de sugerencias
            private async void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
            {
                string texto = e.NewTextValue;

                if (string.IsNullOrWhiteSpace(texto))
                {
                    // No hay texto → mostrar resultados, ocultar sugerencias
                    ResultsCollection.IsVisible = true;
                    SuggestionsList.IsVisible = false;
                }
                else
                {
                    // Hay texto → mostrar sugerencias, ocultar resultados
                    ResultsCollection.IsVisible = false;
                    SuggestionsList.IsVisible = true;

                    // Aquí podrías actualizar las sugerencias
                    await ActualizarSugerencias(texto);
                }
            }

        // Seleccionar una sugerencia
            private void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.CurrentSelection.FirstOrDefault() is string seleccion)
                {
                    if (BindingContext is AndroidPropertyViewModel vm)
                    {
                        txt_search.Unfocus();
                        vm.SearchText = seleccion;
                        SuggestionsList.IsVisible = false;
                        SuggestionsList.SelectedItem = null;
                        _ = vm.Search();
                    }
                }
            }

        // Funciones de privadas
            private async Task ActualizarSugerencias(string texto)
            {
                _cts?.Cancel();             // cancelar la anterior
                _cts = new CancellationTokenSource();

                try
                {
                    var sugerencias = await ObtenerSugerenciasAsync(texto, _cts.Token);
                    SuggestionsList.ItemsSource = sugerencias;
                }
                catch (OperationCanceledException)
                {
                    // Ignorar si fue cancelado
                }
            }
            private static async Task<List<string>> ObtenerSugerenciasAsync(string query, CancellationToken token)
            {
                try
                {

                    if (string.IsNullOrWhiteSpace(query))
                        return [];

                    var url = $"https://suggestqueries.google.com/complete/search?client=firefox&ds=yt&q={Uri.EscapeDataString(query)}";
                    var result = await _http.GetFromJsonAsync<object[]>(url, token);

                    if (result?.Length > 1 && result[1] is JsonElement suggestionsElement && suggestionsElement.ValueKind == JsonValueKind.Array)
                    {
                        return suggestionsElement
                            .EnumerateArray()
                            .Select(x => x.GetString())
                            .OfType<string>()
                            .ToList();
                    }


                } catch (Exception ex) {
                    Debug.WriteLine($"Ocurrio un error en: {ex.Message}");
                }

                return new List<string>();

            }
    }
}
