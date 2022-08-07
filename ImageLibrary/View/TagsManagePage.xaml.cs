using ImageLibrary.ViewModel;
namespace ImageLibrary.View;

public partial class TagsManagePage : ContentPage
{
	public TagsManagePage(TagsManageViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
	}
}