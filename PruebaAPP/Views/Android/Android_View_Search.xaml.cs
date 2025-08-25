using CommunityToolkit.Mvvm.Input;
using PruebaAPP.Objetos.Functions;
using PruebaAPP.Objetos.Models;
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
        }


        // Funciones de items
        private async void Click_SelectedItems(object sender, SelectionChangedEventArgs e)
        {
            // Verificar si hay una selección actual y obtener el primer elemento
            if (e.CurrentSelection.Count == 0 || e.CurrentSelection[0] is not VideoSearchResult selected) return;

            // Limpiar selección visual
            if (sender is CollectionView cv)
                cv.SelectedItem = null;

            // Ejecuta el handler / comando del VM
            if (BindingContext is not MainViewModel vm)
                return;

            // Reproducimos
            await vm.Play(selected.Id);
        }
        private async void Click_SelectedItemMenu(object sender, EventArgs e)
        {
            // Verificamos parametros
            if (sender is not Button btn)
                return;

            // Verificamos contexto del boton
            if (btn.BindingContext is not VideoSearchResult psr)
                return;

            // Verificamos contexto del ViewModel
            if (BindingContext is not MainViewModel vm)
                return;

            // Verificar si el favorito es nulo o no tiene un ID válido
            if (psr is null || string.IsNullOrWhiteSpace(psr.Id)) return;

            // Abrir el menú contextual
            var title  = "Selecciona acción";
            var cancel = "Cancelar";
            var param  = new[] { "Reproducir", "Agregar a favoritos", "Agregar a una playlist" };
            var action = await DialogHelpers.DisplayAction(title, cancel, null, param);

            // Ejecutamos la acción seleccionada
            switch (action)
            {
                case "Reproducir":

                    // Reproducimos
                    await vm.Play(psr.Id);
                    break;
                case "Agregar a favoritos":

                    // Agregamos
                    Song favorito = new()
                    {
                        Id               = psr.Id,
                        Title            = psr.Title,
                        Author           = new AuthorSong { ChannelId = psr.Author.ChannelId, ChannelTitle = psr.Author.ChannelTitle, ChannelUrl = psr.Author.ChannelUrl },
                        ThumbnailHighRes = ThumbnailHelper.GetHighestThumbnail(psr.Thumbnails),
                        ThumbnailLowRes  = ThumbnailHelper.GetLowestThumbnail(psr.Thumbnails),
                        Duration         = psr.Duration
                    
                    };

                    // Ejecutamos
                    if (vm.Favorite_AddCommand.CanExecute(favorito))
                        vm.Favorite_AddCommand.Execute(favorito);
                        break;          
                case "Agregar a una playlist":

                    // Agregamos
                    Song fav = new()
                    {
                        Id          = psr.Id,
                        Title       = psr.Title,
                        Author      = new AuthorSong {ChannelId = psr.Author.ChannelId, ChannelTitle = psr.Author.ChannelTitle, ChannelUrl = psr.Author.ChannelUrl },
                        ThumbnailHighRes = ThumbnailHelper.GetHighestThumbnail(psr.Thumbnails),
                        ThumbnailLowRes = ThumbnailHelper.GetLowestThumbnail(psr.Thumbnails),
                        Duration = psr.Duration
                    
                    };

                    // Ejecutamos
                    if (vm.Playlist_ToAddCommand.CanExecute(fav))
                        vm.Playlist_ToAddCommand.Execute(fav);
                        break;
            }

        }


        // Funcion de buscar
        private async void Search(string input)
        {
            // Verificamos contexto del ViewModel
            if (BindingContext is not MainViewModel vm)
                return;

            // Configuramos controles
            txt_search.Unfocus();
            SuggestionsList.ItemsSource = null;
            SuggestionsList.IsVisible = false;
            SuggestionsList.SelectedItem = null;
            ResultsCollection.IsVisible = true;

            // Buscamos
            await vm.Search(input);

        }
        private void SearchButtonPressed(object sender, EventArgs e)
        {
            var input = txt_search.Text ?? string.Empty;
            Search(input);
        }
        private async void ResultsCollection_RemainingItemsThresholdReached(object sender, EventArgs e)
        {
            // Intentamos invocar el comando del VM
            if (BindingContext is MainViewModel vm)
            {
                try
                {
                    await vm.LoadMore();
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


        // Funcion de sugerencias
        private async void txt_search_TextChanged(object sender, TextChangedEventArgs e)
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
        private void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            // Verificar si hay una selección actual y obtener el primer elemento
            if (e.CurrentSelection.Count == 0 || e.CurrentSelection[0] is not string input)
                return;

            // Ejecuta el handler / comando del VM
            txt_search.Text = input;
            Search(input);

        }
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

                // Validamos entrada
                if (string.IsNullOrWhiteSpace(query))
                    return [];

                // Hacemos peticion
                var url = $"https://suggestqueries.google.com/complete/search?client=firefox&ds=yt&q={Uri.EscapeDataString(query)}";
                var result = await _http.GetFromJsonAsync<object[]>(url, token);

                // Procesamos resulados
                if (result?.Length > 1 && result[1] is JsonElement suggestionsElement && suggestionsElement.ValueKind == JsonValueKind.Array)
                {
                    return suggestionsElement.EnumerateArray()
                                             .Select(x => x.GetString())
                                             .Where(x => x != null)
                                             .ToList()!;
                }


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrio un error en: {ex.Message}");
            }

            // En caso de fallar, lista en blanco.
            return [];

        }


    }
}
