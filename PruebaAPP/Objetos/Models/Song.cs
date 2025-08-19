using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeExplode.Common;

namespace PruebaAPP.Objetos.Models
{
    public partial class Song : ObservableObject
    {
        [ObservableProperty] public partial string? Id                           { get; set; }
        [ObservableProperty] public partial string? Title                        { get; set; }
        [ObservableProperty] public partial Author? Channel                      { get; set; }
        [ObservableProperty] public partial IReadOnlyList<Thumbnail>? Thumbnails { get; set; }
        [ObservableProperty] public partial TimeSpan? Duration                   { get; set; }
        [ObservableProperty] public partial TimeSpan? CurrentTime                { get; set; }
        [ObservableProperty] public partial TimeSpan? TotalTime                  { get; set; }
        [ObservableProperty] public partial double ProgressTime                  { get; set; }
        [ObservableProperty] public partial double? SizeFile                     { get; set; }
        [ObservableProperty] public partial bool? IsFavorite                     { get; set; }
        [ObservableProperty] public partial bool? IsCache                        { get; set; }

    }
}
