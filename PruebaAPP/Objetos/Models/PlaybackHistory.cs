using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PruebaAPP.Objetos.Models
{
    public class PlaybackHistory
    {
        // Identificador único
        public required int Id { get; set; }

        // Relación con la canción
        public string? Title { get; set; }
        public string? Artist { get; set; }

        // Datos de reproducción
        public DateTime PlayedAt { get; set; }
        public TimeSpan DurationPlayed { get; set; }
        public bool IsCompleted { get; set; }


    }
}
