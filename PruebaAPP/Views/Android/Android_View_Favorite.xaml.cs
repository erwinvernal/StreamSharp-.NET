using Microsoft.Maui.Controls.Platform.Compatibility;
using PruebaAPP.Objetos.Models;
using PruebaAPP.Views.Android.View_Favorite;
using PruebaAPP.Views.Android.ViewModels;
using System.Diagnostics;
using YoutubeExplode.Search;

namespace PruebaAPP.Views.Android;

public partial class Android_View_Favorite : ContentView
{
	public Android_View_Favorite()
	{
		InitializeComponent();
        ContainerView.Content = new Android_Favorite_Song();

        rb0.IsChecked = true; // Seleccionar por defecto
    }

    private void Click_Changetype(object sender, CheckedChangedEventArgs e)
    {
        if (BindingContext is AndroidPropertyViewModel vm && e.Value) // Solo cuando se selecciona
        {
            if (sender is RadioButton rb && int.TryParse(rb.ClassId, out int tipo))
            {
                switch (tipo)
                {
                    case 0: ContainerView.Content = new Android_Favorite_Song(); break;
                    case 1: ContainerView.Content = new Android_Favorite_Playlist(); break;
                    default: break;
                }
            }
        }
    }
}