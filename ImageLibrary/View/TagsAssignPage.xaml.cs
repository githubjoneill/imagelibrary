using CommunityToolkit.Maui.Core.Extensions;

namespace ImageLibrary.View;

public partial class TagsAssignPage : ContentPage
{
	ImageInfoViewModel _vm;
	public TagsAssignPage(ImageInfoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_vm = viewModel;
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);
        if (_vm != null && _vm.TagMultiAssignments!=null )
        {
			_vm.TagsAssigned = _vm.TagMultiAssignments.tagsAssigned.ToObservableCollection<Tag>();
			_vm.TagsAvailable = _vm.TagMultiAssignments.tagsAvailable.ToObservableCollection<Tag>();
        }
    }
}