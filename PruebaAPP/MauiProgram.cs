using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Controls.Embedding;
using SkiaSharp.Views.Maui.Controls.Hosting;

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
            .UseMauiCommunityToolkitCore()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
             {
                 fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                 fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                 fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIconsRegular");
                 fonts.AddFont("MaterialIconsOutlined-Regular.ttf", "MaterialIconsOutlinedRegular");
             });

            return builder.Build();
        }

    }
}