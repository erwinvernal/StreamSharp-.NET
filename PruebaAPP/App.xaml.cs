using PruebaAPP.Views.Android;

namespace PruebaAPP
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var win = new Window(new Android_Page_Main())
            {
                Width = 700,
                Height = 800
                
            };

            return win;
        }
    }
}