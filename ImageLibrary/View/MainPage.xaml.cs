//using Android.OS;

namespace ImageLibrary.View;

public partial class MainPage : ContentPage
{
	//int count = 0;

	public MainPage(ImageInfoViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;

        
    }

   
}

