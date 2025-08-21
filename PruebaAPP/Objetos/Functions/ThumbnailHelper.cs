using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Common;

namespace PruebaAPP.Objetos.Functions
{
    public static class ThumbnailHelper
    {
        public static string[] GetLowestAndHighestThumbnails(IEnumerable<Thumbnail> thumbnails)
        {
            if (thumbnails == null || !thumbnails.Any())
                return Array.Empty<string>();

            // Ignoramos thumbnails nulos
            var validThumbs = thumbnails.Where(t => t != null).ToList();

            if (!validThumbs.Any())
                return Array.Empty<string>();

            // Ordenamos por ancho para encontrar la menor y mayor resolución
            var lowest = validThumbs
                .OrderBy(t => t.Resolution.Width * t.Resolution.Height)
                .FirstOrDefault()?.Url;

            var highest = validThumbs
                .OrderByDescending(t => t.Resolution.Width * t.Resolution.Height)
                .FirstOrDefault()?.Url;

            // Aseguramos que nunca haya null en el array, reemplazando por string vacío si es necesario
            return new string[]
            {
                lowest ?? string.Empty,
                highest ?? string.Empty
            };
        }
    }
}
