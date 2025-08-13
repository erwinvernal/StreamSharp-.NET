using PruebaAPP.Controls;
using PruebaAPP.Views.Android;

namespace PruebaAPP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Handler
                Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(EntrySB), (handler, view) =>
                {
#if __ANDROID__
                    handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif __IOS__
                    handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#elif WINDOWS
                    handler.PlatformView.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                    Microsoft.UI.Xaml.Media.SolidColorBrush transparentbrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                    handler.PlatformView.BorderBrush = transparentbrush;
#endif              
                });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new Android_Page_Main());
        }
    }
}