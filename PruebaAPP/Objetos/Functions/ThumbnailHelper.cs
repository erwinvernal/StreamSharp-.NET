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
        public static string GetLowestThumbnail(IEnumerable<Thumbnail>? thumbnails)
        {
            if (thumbnails == null || !thumbnails.Any())
                return string.Empty;

            var validThumbs = thumbnails.Where(t => t != null).ToList();
            if (!validThumbs.Any())
                return string.Empty;

            var lowest = validThumbs
                .OrderBy(t => t.Resolution.Width * t.Resolution.Height)
                .FirstOrDefault()?.Url;

            return lowest ?? string.Empty;
        }

        public static string GetHighestThumbnail(IEnumerable<Thumbnail>? thumbnails)
        {
            if (thumbnails == null || !thumbnails.Any())
                return string.Empty;

            var validThumbs = thumbnails.Where(t => t != null).ToList();
            if (!validThumbs.Any())
                return string.Empty;

            var highest = validThumbs
                .OrderByDescending(t => t.Resolution.Width * t.Resolution.Height)
                .FirstOrDefault()?.Url;

            return highest ?? string.Empty;
        }
    }
}
