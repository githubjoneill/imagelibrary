

using CommunityToolkit.Maui;
using ImageLibrary.Services;
using ImageLibrary.View;

namespace ImageLibrary;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
		builder.Services.AddSingleton<ImageInfoService>();
        builder.Services.AddSingleton<TagService>();

        builder.Services.AddTransient<ImageInfoViewModel>();
        builder.Services.AddTransient<MainPage>();

		builder.Services.AddTransient<ImageDetailsViewModel>();
		builder.Services.AddTransient<DetailsPage>();

		builder.Services.AddTransient<TagsManageViewModel>();
		builder.Services.AddTransient<TagsManagePage>();

        builder.Services.AddTransient<ImagesWithoutTags>();
		builder.Services.AddTransient<TagsAssignPage>();

        return builder.Build();
	}
}
