using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using YoutubeExplode.Common;

namespace PruebaAPP.Objetos.Models
{
    public partial class Song : ObservableObject
    {
        [ObservableProperty] public partial string? Id                           { get; set; }
        [ObservableProperty] public partial string? Title                        { get; set; }
        [ObservableProperty] public partial Author? Author                       { get; set; }
        [ObservableProperty] public partial IReadOnlyList<Thumbnail>? Thumbnails { get; set; }
        [ObservableProperty] public partial TimeSpan? Duration                   { get; set; }

        // Atributos no guardables
        [JsonIgnore][ObservableProperty] public partial TimeSpan? CurrentTime { get; set; }
        [JsonIgnore][ObservableProperty] public partial TimeSpan? TotalTime { get; set; }
        [JsonIgnore][ObservableProperty] public partial double ProgressTime { get; set; }
        [JsonIgnore][ObservableProperty] public partial double? SizeFile { get; set; }
        [JsonIgnore][ObservableProperty] public partial bool IsFavorite { get; set; }
        [JsonIgnore][ObservableProperty] public partial bool IsCache { get; set; }
        [JsonIgnore][ObservableProperty] public partial bool IsPlay { get; set; }

    }
}
