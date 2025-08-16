using System.Collections.Generic;             // 👈 necesario para IAsyncEnumerator
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

public class YouTubeService
{
    private readonly YoutubeClient _youtube = new();

    // No es async: entregar el enumerador no requiere await
    public IAsyncEnumerator<VideoSearchResult> GetSearchEnumerator(string query)
        => _youtube.Search.GetVideosAsync(query).GetAsyncEnumerator();

    public async Task<IStreamInfo?> GetBestAudioStreamAsync(string videoUrl)
    {
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoUrl);
        return manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
    }

    public async Task<VideoSearchResult> GetVideoDetailsAsync(string videoId)
    {
        Video a = await _youtube.Videos.GetAsync(videoId);
        VideoSearchResult b = new(a.Id, a.Title, a.Author, a.Duration, a.Thumbnails);
        return b;
    }
}
