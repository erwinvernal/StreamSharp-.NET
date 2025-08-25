using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Objetos.Services;
using PruebaAPP.Views.Android.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaAPP.ViewModels
{
    public partial class PlayerViewModel : ObservableObject
    {
        // =============================================================================================
        // == Inicialización del viewmodel media
        // =============================================================================================
        public PlayerViewModel(MediaElement player)
        {
            // Inicializar el reproductor
            this.Controller = player;
            this.Controller.MediaEnded       += Player_MediaEnded;
            this.Controller.MediaFailed      += Player_MediaFailed;
            this.Controller.MediaOpened      += Player_MediaOpened;
            this.Controller.StateChanged     += Player_StateChanged;
            this.Controller.PositionChanged  += Player_PositionChanged;
        }

        public MainViewModel? ViewM { get; set; }
        public MediaElement Controller { get; private set; }

        // =============================================================================================
        // == Propiedades
        // =============================================================================================
        [ObservableProperty] public partial Song? CurrentSong                   { get; set; } = null;                      // Canción actual
        [ObservableProperty] public partial Playlist SelectedPlaylist           { get; set; } = new Playlist();             // Playlist seleccionada
        [ObservableProperty] public partial Playlist ViewPlaylist               { get; set; } = new Playlist();             // Playlist que se está visualizando
        [ObservableProperty] public partial MediaElementState CurrentMediaState { get; set; } = MediaElementState.None;      // Estado actual del reproductor
        [ObservableProperty] public partial int CurrentSongIndex                { get; set; } = 0;                          // Índice de la canción actual en la lista

        // =============================================================================================
        // == Funciones auxiliares
        // =============================================================================================
        [ObservableProperty] public partial double Position { get; set; }                                                   // Posición actual del reproductor en segundos
        [ObservableProperty] public partial double Duration { get; set; }                                                   // Duración total del medio en segundos

        // =============================================================================================
        // == Eventos del reproductor
        // =============================================================================================
        private void Player_MediaFailed(object? sender, MediaFailedEventArgs e)
        {
            CurrentMediaState = MediaElementState.Failed;
        }
        private void Player_StateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            CurrentMediaState = e.NewState;
        }
        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            Duration = Controller.Duration.TotalSeconds;
        }
        private void Player_PositionChanged(object? sender, MediaPositionChangedEventArgs e)
        {
            Position = e.Position.TotalSeconds;
            Duration = Controller.Duration.TotalSeconds;
        }
        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            if ((Position > 0) && (Duration > 0))
            {
                ViewM!.Media_SkipNext();
            }
        }

    }
}
