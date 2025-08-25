using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace PruebaAPP.Objetos.Models
{
    public partial class Song : ObservableObject
    {
        public string? Id               { get; set; } = null;
        public string? Title            { get; set; } = null;
        public AuthorSong? Author       { get; set; } = null;
        public string? ThumbnailHighRes { get; set; } = null;
        public string? ThumbnailLowRes  { get; set; } = null;
        public TimeSpan? Duration       { get; set; } = null;

        // Atributos no guardables
        [JsonIgnore][ObservableProperty] public partial double? SizeFile { get; set; } = null;

        // Atributos de estado
        [JsonIgnore][ObservableProperty] public partial bool IsFavorite { get; set; } = false;
        [JsonIgnore][ObservableProperty] public partial bool IsCache { get; set; } = false;
        [JsonIgnore][ObservableProperty] public partial bool IsPlay { get; set; } = false;

    }
}
