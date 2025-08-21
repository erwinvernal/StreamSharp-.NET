using PruebaAPP.Views.Android.ViewModels;

namespace PruebaAPP.Views.Android;

public partial class Android_View_Control : ContentView
{
	public Android_View_Control()
	{
		InitializeComponent();
	}

    private void Click_ControlPlayer(object sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
        {
            if (sender is Button rb && int.TryParse(rb.ClassId, out int tipo))
            {
                // Seleccionamos accion
                switch (tipo)
                {
                    case 0: 
                    case 1: 
                    case 2:  
                    case 3: 
                    default: break;
                }
            }
        }
    }

}