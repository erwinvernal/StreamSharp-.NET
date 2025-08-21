using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Objetos.Services;
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
            Controller = player;
            Controller.MediaEnded       += Player_MediaEnded;
            Controller.MediaFailed      += Player_MediaFailed;
            Controller.MediaOpened      += Player_MediaOpened;
            Controller.StateChanged     += Player_StateChanged;
            Controller.PositionChanged  += Player_PositionChanged;
        }
        public MediaElement Controller { get; private set; }

        // =============================================================================================
        // == Variables publicas
        // =============================================================================================
        public bool IsBlockClick { get; set; } = false;


        // =============================================================================================
        // == Propiedades
        // =============================================================================================
        [ObservableProperty] public partial Song CurrentSong { get; set; } = new Song();
        [ObservableProperty] public partial Playlist? CurrentPlaylist { get; set; } = new Playlist();
        [ObservableProperty] public partial MediaElementState CurrentMediaState { get; set; } = MediaElementState.Stopped;
        [ObservableProperty] public partial int CurrentSongIndex { get; set; } = 0;

        // =============================================================================================
        // == Funciones auxiliares
        // =============================================================================================
        [ObservableProperty] public partial double Position { get; set; }
        [ObservableProperty] public partial double Duration { get; set; }

        // =============================================================================================
        // == Eventos del reproductor
        // =============================================================================================
        private void Player_MediaFailed(object? sender, MediaFailedEventArgs e)
        {
            // Manejar el error de reproducción
            CurrentMediaState = MediaElementState.Stopped;
            // Aquí podrías mostrar un mensaje al usuario o registrar el error
        }
        private void Player_StateChanged(object? sender, MediaStateChangedEventArgs e)
        {
            // Actualizar el estado actual del reproductor
            CurrentMediaState = e.NewState;
        }
        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            // Cuando se abre un medio, se actualiza la duración
            Duration = Controller.Duration.TotalSeconds;
        }
        private void Player_PositionChanged(object? sender, MediaPositionChangedEventArgs e)
        {
            // Actualizar la posición actual
            Position = e.Position.TotalSeconds;
            Duration = Controller.Duration.TotalSeconds;

            // Notificamos cambios
            //OnPropertyChanged(nameof(Position));
            //OnPropertyChanged(nameof(Duration));
        }
        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            // Cuando la canción termina, se detiene el reproductor
            //Controller.Stop();
            CurrentMediaState = MediaElementState.Stopped;
        }

    }
}
