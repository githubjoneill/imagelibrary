namespace ImageLibrary.View;
//using CommunityToolkit.Maui.Core.Extensions;

public partial class BrowseByTagPage : ContentPage
{
	public BrowseByTagPage(ImageTagGroupViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

}