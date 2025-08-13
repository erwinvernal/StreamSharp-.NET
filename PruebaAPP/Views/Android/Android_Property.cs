using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace PruebaAPP.Views.Android
{
    public partial class Android_Property : ObservableObject
    {

        // Almacenamiento
            public readonly System.Timers.Timer Refresh = new(500);
            public MediaElement? mPlayer;

        // Inicializacion
            public Android_Property() 
            {

                // Enlzamos evento del timer
                    Refresh.Elapsed += Refresh_Elapsed;
            }

        // Propiedades de binding
            [ObservableProperty]
            public partial string? TitleSong { get; set; }
            
            [ObservableProperty]
            public partial string? Channel { get; set; }
            
            [ObservableProperty]
            public partial string? Thumbnails { get; set; }
            
            [ObservableProperty]
            public partial TimeSpan? CurrentTime {  get; set; }
            
            [ObservableProperty]
            public partial TimeSpan? TotalTime {  get; set; }
            
            [ObservableProperty]
            public partial double ProgressTime { get; set; }
            
            [ObservableProperty]
            public partial IReadOnlyList<VideoSearchResult>? ListItems {  get; set; }

        // Funciones del timer
        private async void Refresh_Elapsed(object? sender, ElapsedEventArgs e)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Verificamos si no mandamos un error
                        if (mPlayer is null) { throw new Exception("Al parecer no se inicializo el reproductor."); };

                    // Actualizamos valores
                        CurrentTime  = mPlayer.Position;
                        TotalTime    = mPlayer.Duration;
                        if (CurrentTime > TimeSpan.Zero && mPlayer.Duration > TimeSpan.Zero)
                        {
                            ProgressTime = mPlayer.Position.TotalSeconds / mPlayer.Duration.TotalSeconds;
                        } else {
                            ProgressTime = 0.0f;
                        }
                });
            }
    }
}
