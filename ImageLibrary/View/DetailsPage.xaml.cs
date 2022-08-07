
namespace ImageLibrary;//.View

public partial class DetailsPage : ContentPage
{
    ImageDetailsViewModel _vm;
	public DetailsPage(ImageDetailsViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
        _vm = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (_vm !=null)
        {
            _vm.AvailableTags = _vm.FullyLoadedImage.AvailableTags;
            _vm.Tags = _vm.FullyLoadedImage.AssignedTags; 
        }
    }
}