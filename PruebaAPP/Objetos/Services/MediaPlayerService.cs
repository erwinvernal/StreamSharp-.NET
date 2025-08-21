using CommunityToolkit.Maui.Views;

namespace PruebaAPP.Objetos.Services
{
    public class MediaPlayerService(MediaElement player)
    {
        public MediaElement Player { get; private set; } = player;

        public void Play(string url)
        {
            Player.Source = url;
            Player.Play();
        }

        public void Pause() => Player.Pause();

        public void Stop() => Player.Stop();

        public void Seek(TimeSpan position) => Player.SeekTo(position);

        public TimeSpan CurrentTime => Player.Position;
        public TimeSpan TotalTime => Player.Duration;
    }

}
