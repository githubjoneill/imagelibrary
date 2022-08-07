namespace ImageLibrary;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();

		//MainPage.NavigatedTo += MainPage_NavigatedTo;
	}

	//private void MainPage_NavigatedTo(object sender, NavigatedToEventArgs e)
	//{
	//	var s = sender;

	//	var eArgs = e;
	//	//throw new NotImplementedException();
	//}
}
