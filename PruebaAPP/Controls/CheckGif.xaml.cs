using SkiaSharp.Extended.UI.Controls;

namespace PruebaAPP.Controls;

public partial class CheckGif : ContentView
{
    // Propiedad Bindable
    public static readonly BindableProperty SourceProperty      = BindableProperty.Create(nameof(Source)   , typeof(SKLottieImageSource), typeof(CheckGif), null);
    public static readonly BindableProperty IsCheckedProperty   = BindableProperty.Create(nameof(IsChecked), typeof(bool)       , typeof(CheckGif), false);

    // Propiedad del control
    public SKLottieImageSource Source
    {
        get => (SKLottieImageSource)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set 
        {
            SetValue(IsCheckedProperty, value);
        }
    }

    // Inicializacion del control
	public CheckGif()
	{
		InitializeComponent();
    }

    // Evento de click
    private void OnGifTapped(object sender, TappedEventArgs e)
    {
        IsChecked = !IsChecked;
        AnimationLottie.IsAnimationEnabled = true;
    }

    private void AnimationLottie_AnimationCompleted(object sender, EventArgs e)
    {
        AnimationLottie.IsAnimationEnabled = false;
        AnimationLottie.Progress = TimeSpan.Zero;
    }
}