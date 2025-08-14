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


    // Funciones de youtube explode

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