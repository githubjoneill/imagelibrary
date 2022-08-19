using ImageLibrary.View;

namespace ImageLibrary;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage));

		Routing.RegisterRoute(nameof(TagsManagePage), typeof(TagsManagePage));

		Routing.RegisterRoute(nameof(BrowseByTagPage), typeof(BrowseByTagPage));

		Routing.RegisterRoute(nameof(TagsAssignPage), typeof(TagsAssignPage));

    }
}
