namespace PruebaAPP.Objetos.Models
{
    public enum FType
    {
        Canal,
        Playlist,
        Cancion
    }

    public partial class Favorite
    {
        public FType   Type   { get; set; }  // Tipo de recurso (Canal, Playlist, Canción)
        public string? Id     { get; set; }  // Id del recurso
        public string? Title  { get; set; }  // Nombre 
        public string? Author { get; set; }  // Opcional
        public string? Thumbnails { get; set; }  // Opcional
        public string? Duracion { get; set; }  // Opcional
    }

}
