using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;  // Para MainThread
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace PruebaAPP
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object? sender, EventArgs e)
        {
            try
            {
                await ProcesarBusquedaAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async Task ProcesarBusquedaAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Txt_Search.Text))
            {
                Debug.WriteLine("No puede estar en blanco.");
                return;
            }
            else if (!this.Txt_Search.Text.Contains("https://"))
            {
                Debug.WriteLine("No es una url");
                return;
            }

            var url = this.Txt_Search.Text;
            var youtube = new YoutubeClient();

            // Evitar NetworkOnMainThreadException
            await Task.Run(async () =>
            {
                var manifest = await youtube.Videos.GetAsync(url);
                var audioManifest = await youtube.Videos.Streams.GetManifestAsync(url);

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
