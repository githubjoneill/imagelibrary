namespace ImageLibrary.View;
//using CommunityToolkit.Maui.Core.Extensions;

public partial class BrowseByTagPage : ContentPage
{
	//ImageInfoViewModel _vm;
	public BrowseByTagPage(ImageTagGroupViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
	//	_vm = viewModel;
    }

	//protected  override void  OnNavigatedTo(NavigatedToEventArgs args)
	//{
	//	base.OnNavigatedTo(args);

	//	//if (_vm !=null)
	//	//{

			
	//	//	if (_vm.TagGroups?.Count ==0)
	//	//	{
	//	//		var allTags = await _vm.tagService.GetTags();
	//	//		if (allTags?.Count ==0)
	//	//		{
	//	//			return;
	//	//		}

	//	//		foreach (var tag in allTags)
	//	//		{

	//	//		}
	//	//		var lstFiles = await _vm.imageInfoService.GetUntaggedImages();
	//	//		var tf  = new List<TagImageGroup>();
	//	//		var grp = new TagImageGroup("Untagged", lstFiles);

	//	//		//tf.Add(new TagImageGroup("Untagged", lstFiles));
	//	//		_vm.TagGroups.Add(grp);
	//	//		//_vm.TagGroups = tf;

	//	//		//tf.Add(new Tag { Name = "No tags" });
	//	//		//            tf.Add(new Tag { Name = "Some other value" });

				

  
	//}
}