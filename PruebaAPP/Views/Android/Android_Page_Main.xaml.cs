using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;
using CommunityToolkit.Mvvm.Input;

namespace PruebaAPP.Views.Android;
public partial class Android_Page_Main : ContentPage
{

    // Propiedades de la clase
        // Almacenamiento
            private readonly Android_Property vm = new();
            bool issearch;

    // Inicializacion de la pagina
        public Android_Page_Main()
	        {

                // Inicializacion
		            InitializeComponent();          // Inicializamos XAML
                    this.BindingContext = vm;       // Establecemos Binding
                    this.vm.mPlayer     = PlayerM;  // Establecemos reproductor

                // Mostramos contenido
                    this.ModuleContainer.Content = new Android_View_Search(vm);

	        }

    // Funciones de la pagina
        private async void Click_Search(object sender, EventArgs e)
        {
            await Search();
        }

    // Funciones de youtube explode
        [RelayCommand]
        async Task Search()
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
                        vm.ListItems = null;

                    // Inicializamos yte
                        var youtube = new YoutubeClient();
                        var search = this.txt_search.Text;

                    // Creamos lista
                        var result = await youtube.Search.GetVideosAsync(search).CollectAsync(20);

                    // Verificamos si se cancelo
                        //if (!token.IsCancellationRequested) { return; }

                    // Copiamos resultado
                        vm.ListItems = result;

                    // Borramos busqueda
                        this.txt_search.Text = null;

                } catch (Exception ex) {
                    Debug.WriteLine(ex.Message);

                } finally {
                    this.issearch = false;
                }
        }

    // Funciones del reproductor
        private void MediaOpen_PlayerM(object sender, EventArgs e)
        {
            this.vm.Refresh.Start();
            Debug.WriteLine("Refresh Iniciado.");
        }
        private void MediaEnded_PlayerM(object sender, EventArgs e)
        {
            // Detenemos refresh
                this.vm.Refresh.Stop();
                Debug.WriteLine("Refresh Detenido.");

            // Quitamos todo los parametros
                this.vm.Thumbnails = null;
                this.vm.TitleSong = null;
                this.vm.Channel = null;
                this.vm.CurrentTime = TimeSpan.Zero;
                this.vm.TotalTime = TimeSpan.Zero;
                
        }


}