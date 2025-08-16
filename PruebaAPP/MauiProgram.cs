using CommunityToolkit.Maui;

namespace PruebaAPP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
             {
                 fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                 fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                 fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIconsRegular");
                 fonts.AddFont("MaterialIconsOutlined-Regular.ttf", "MaterialIconsOutlinedRegular");
                 // Puedes agregar fuentes de íconos aquí también
             });

            return builder.Build();
        }

    }
}