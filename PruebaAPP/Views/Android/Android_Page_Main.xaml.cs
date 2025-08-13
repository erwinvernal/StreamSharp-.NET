using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace PruebaAPP.Views.Android;
public partial class Android_Page_Main : ContentPage
{

    // Binding
        public static readonly BindableProperty ListItemsProperty = BindableProperty.Create(
                nameof(ListItems),                     // Nombre de la propiedad
                typeof(IReadOnlyList<YoutubeExplode.Search.VideoSearchResult>), // Tipo
                typeof(Android_Page_Main),             // Tipo de la clase que la contiene
                new ObservableCollection<YoutubeExplode.Search.VideoSearchResult>()    // Valor por defecto
            );

    // Propiedades de la clase
        public IReadOnlyList<YoutubeExplode.Search.VideoSearchResult>? ListItems
        {
            get => (IReadOnlyList<YoutubeExplode.Search.VideoSearchResult>)GetValue(ListItemsProperty);
            set => SetValue(ListItemsProperty, value);
        }

    // Variables locales
        bool issearch;

    // Inicializacion de la pagina
    public Android_Page_Main()
	    {
            // Suscribirse al evento Loaded si es necesario
                this.Loaded += ThisLoaded;  

            // Inicializacion
                this.BindingContext = this;
		        InitializeComponent();

	    }

    // Funciones de la pagina
        private void ThisLoaded(object? sender, EventArgs e)
        {
            Debug.WriteLine("Sin uso por ahora");
        }

    // Funciones de youtube explode
        private async void Click_Search(object sender, EventArgs e)
        {
            // Criterio de cancelacion
                if (this.issearch) { return; }

            // Bloqueamos control
                this.issearch = true;

            // Iniciamos try
                try
                {
                    // Criterios de cancelacion
                        if (string.IsNullOrWhiteSpace(this.txt_search.Text)) { return; }

                    // Reseteamos
                        this.ListItems = null;

                    // Inicializamos yte
                        var youtube = new YoutubeClient();
                        var search = this.txt_search.Text;

                    // Creamos lista
                        var result = await youtube.Search.GetVideosAsync(search).CollectAsync(20);
                        this.ListItems = result;

                    // Borramos busqueda
                        this.txt_search.Text = null;

                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);
                } finally {
                    this.issearch = false;
                }
        }
        private async void OnBorderTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border ctrl && ctrl.BindingContext is YoutubeExplode.Search.VideoSearchResult item)
            {
                // Aquí tienes tu objeto con los datos
                    var link = item.Url;

                // Ejemplo: mostrarlo en consola
                    var youtube = new YoutubeClient();

                // Evitar NetworkOnMainThreadException
                    await Task.Run(async () =>
                    {
                        var manifest = await youtube.Videos.GetAsync(link);
                        var audioManifest = await youtube.Videos.Streams.GetManifestAsync(link);

                        var stream = audioManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            this.MediaP.Source = stream.Url;
                            this.MediaP.Play();
                        });

                        Debug.WriteLine($"Reproduciendo: {manifest.Title} - {manifest.Author.ChannelTitle}");
                    });
        }
    }

}