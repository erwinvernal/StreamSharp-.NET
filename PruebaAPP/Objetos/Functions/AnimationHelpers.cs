namespace PruebaAPP.Objetos.Functions
{
    public static class AnimationHelpers
    {
        public static async Task PlayRipple(Grid grid)
        {
            if (grid == null)
                return;

            // Animación de “presionado”
            await grid.ScaleTo(0.95, 100, Easing.CubicOut);
            await grid.ScaleTo(1, 100, Easing.CubicIn);
        }
    }
}
