using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PruebaAPP.Objetos.Models
{
    public partial class Song : ObservableObject
    {
        public string? Id           { get; set; }
        public string? Title        { get; set; }
        public string? Author       { get; set; }
        public string? Thumbnails   { get; set; }
        public TimeSpan? Duration   { get; set; }

        // Atributos no guardables
        [JsonIgnore][ObservableProperty] public partial TimeSpan? CurrentTime   { get; set; }
        [JsonIgnore][ObservableProperty] public partial TimeSpan? TotalTime     { get; set; }
        [JsonIgnore][ObservableProperty] public partial double? ProgressTime    { get; set; }
        [JsonIgnore] public double? SizeFile        { get; set; }

        // Atributos de estado
        [JsonIgnore][ObservableProperty] public partial bool IsFavorite { get; set; }
        [JsonIgnore][ObservableProperty] public partial bool IsCache    { get; set; }
        [JsonIgnore][ObservableProperty] public partial bool IsPlay     { get; set; }

    }
}
