using CommunityToolkit.Maui.Views;

namespace PruebaAPP.Objetos.Services
{
    public class MediaPlayerService
    {
        public MediaElement Player { get; private set; }

        public MediaPlayerService(MediaElement player)
        {
            Player = player;
        }

        public void Play(string url)
        {
            Player.Source = url;
            Player.Play();
        }

        public void Pause()
        {
            Player.Pause();
        }

        public TimeSpan CurrentTime => Player.Position;
        public TimeSpan TotalTime => Player.Duration;
    }

}
