﻿using ImageLibrary.View;

namespace ImageLibrary;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(DetailsPage), typeof(DetailsPage));

		Routing.RegisterRoute(nameof(TagsManagePage), typeof(TagsManagePage));

		Routing.RegisterRoute(nameof(ImagesWithoutTags), typeof(ImagesWithoutTags));

    }
}
