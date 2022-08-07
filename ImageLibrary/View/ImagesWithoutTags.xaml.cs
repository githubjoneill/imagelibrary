//using AndroidX.Lifecycle;
using ImageLibrary.ViewModel;

namespace ImageLibrary.View;

public partial class ImagesWithoutTags : ContentPage
{
	public ImagesWithoutTags(ImageInfoViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}