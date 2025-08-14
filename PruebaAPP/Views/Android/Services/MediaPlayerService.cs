using CommunityToolkit.Maui.Views;

namespace PruebaAPP.Views.Android.Services
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
            Player.Stop();
            Player.Source = url;
            Player.Play();
        }

        public void Stop()
        {
            Player.Stop();
            Player.Source = null;
        }

        public TimeSpan CurrentTime => Player.Position;
        public TimeSpan TotalTime => Player.Duration;
    }

}
