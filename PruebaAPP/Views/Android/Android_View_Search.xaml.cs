using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using CommunityToolkit.Maui.Views;

namespace PruebaAPP.Views.Android;

public partial class Android_View_Search : ContentView
{
    // Propiedades de la clase
        public Android_Property? vm { get; set; }

    // Inicializacion
	    public Android_View_Search(Android_Property vm)
	    {
            this.vm = vm;
            this.BindingContext = this.vm;
		    InitializeComponent();
	    }

    // Funciones de la clase
        private async void OnBorderTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border ctrl && ctrl.BindingContext is YoutubeExplode.Search.VideoSearchResult item)
            {

                // Verificamos que no sea null
                    if (vm is null) { throw new Exception("no se inicializo el modalview."); }
                    if (vm.mPlayer is null) { throw new Exception("No se inicializo el reproductor."); }


                // Reiniciamos controles
                    vm.mPlayer.Stop();
                    vm.mPlayer.Source = null;
                    vm.Thumbnails = null;
                    vm.TitleSong = "Cargando...";
                    vm.Channel = null;
                    vm.CurrentTime = TimeSpan.Zero;
                    vm.TotalTime = TimeSpan.Zero;

                // Preparación de la funcion
                    IStreamInfo? StreamInfo = null;
                    StreamManifest? StreamManifest = null;

                // Variables de la funcion
                    var youtube = new YoutubeClient();  // Inicializacion
                    var link = item.Url;             // Url

                // Evitar NetworkOnMainThreadException
                    try
                    {
                        await Task.Run(async () => {
                            StreamManifest = await youtube.Videos.Streams.GetManifestAsync(link);
                            StreamInfo = StreamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                        });
                    } catch (Exception ex) {
                        Debug.WriteLine(ex.Message);
                        return;
                    }

                // Ejecutamos Audio
                    if (StreamInfo is null) { return; }
                    if (StreamManifest is null) { return; }

                // Establecemos informacion
                    vm.Thumbnails = item.Thumbnails[2].Url;
                    vm.TitleSong = item.Title;
                    vm.Channel = item.Author.ChannelTitle;

                // Reproducimos
                    vm.mPlayer.Source = StreamInfo.Url;
                    vm.mPlayer.Play();

            }
        }

}