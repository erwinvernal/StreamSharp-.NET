using CommunityToolkit.Mvvm.ComponentModel;
using YoutubeExplode.Common;

namespace PruebaAPP.Objetos.Models
{
    public partial class Song : ObservableObject
    {
        // Propiedades de la clase
            [ObservableProperty] public partial string? Id { get; set; }
            [ObservableProperty] public partial string? Title { get; set; }
            [ObservableProperty] public partial Author? Channel { get; set; }
            [ObservableProperty] public partial IReadOnlyList<Thumbnail>? Thumbnails { get; set; }
            [ObservableProperty] public partial TimeSpan? Duration { get; set; }
            [ObservableProperty] public partial TimeSpan? CurrentTime { get; set; }
            [ObservableProperty] public partial TimeSpan? TotalTime { get; set; }
            [ObservableProperty] public partial double ProgressTime { get; set; }
            [ObservableProperty] public partial bool? Favorite { get; set; }

        // Propiedad formateada dinámica
            public string CurrentTimeFormatted
            {
            get
            {
                if (CurrentTime is null || Duration is null) return "00:00";
                    if (Duration.Value.TotalHours >= 1)
                        return CurrentTime.Value.ToString(@"hh\:mm\:ss");
                    return CurrentTime.Value.ToString(@"mm\:ss");
                }
            }
            public string TotalTimeFormatted
            {
                get
                {
                    if (Duration is null || TotalTime is null) return "00:00";
                    if (Duration.Value.TotalHours >= 1)
                        return TotalTime.Value.ToString(@"hh\:mm\:ss");
                    return TotalTime.Value.ToString(@"mm\:ss");
                }
            }

        // Avisar cuando cambien CurrentTime o TotalTime
            partial void OnCurrentTimeChanged(TimeSpan? value)
            {
                OnPropertyChanged(nameof(CurrentTimeFormatted));
            }

            partial void OnTotalTimeChanged(TimeSpan? value)
            {
                OnPropertyChanged(nameof(TotalTimeFormatted));
                OnPropertyChanged(nameof(CurrentTimeFormatted)); // porque puede cambiar el formato de ambos
            }
    }
}
