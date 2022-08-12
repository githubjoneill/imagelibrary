using CommunityToolkit.Mvvm.Messaging;
using ImageLibrary.Data;
using ImageLibrary.Messages;
using ImageLibrary.Services;
//using Java.Security;
//using ImageLibrary.View;

namespace ImageLibrary.ViewModel;



public partial class ImageInfoViewModel: BaseViewModel, IRecipient<DeletedImageMessage>, IRecipient<ChangedImageMessage>
{

    
    public ObservableCollection<ImageFileInfo> ImageFiles { get; } = new();

    public ObservableCollection<ImageFileInfo> UntaggedImageFiles { get; } = new();


    public List<Object> SelectedImageFiles { get; set; } = new();

    ImageInfoService imageInfoService;
    IConnectivity connectivity;

    public ImageInfoViewModel(ImageInfoService infoService, IConnectivity connectivity)
    {
        Title = "Image Library";
        this.imageInfoService = infoService;
        this.connectivity = connectivity;

        Task.Run(() => GetImagesAsync());

        WeakReferenceMessenger.Default.Register<DeletedImageMessage>(this);
        WeakReferenceMessenger.Default.Register<ChangedImageMessage>(this);
      
        //this.SelectedImageFiles.CollectionChanged += SelectedImageFiles_CollectionChanged;

    }

    private void SelectedImageFiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var x = sender;
        var y = e.NewItems.Count; 

    }

    [ObservableProperty]
    bool isRefreshing;

    [NotifyPropertyChangedFor(nameof(ResultsInfo))]
    [ObservableProperty]
    int selectionCount;

    [ObservableProperty]
    string resultsInfo  ;

    [RelayCommand]
    async Task GetImagesAsync()
    {
        if (IsBusy)
            return;

        try
        {
            if (connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await Shell.Current.DisplayAlert("No connectivity!",
                    $"Please check internet and try again.", "OK");
                return;
            }
            
            IsBusy = true;
            var images = await imageInfoService.GetImagesInfo();

            if (ImageFiles.Count != 0)
                ImageFiles.Clear();

            foreach (var img in images)
                ImageFiles.Add(img);

           // await UpdateResultsInfo();

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get images: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task SearchImages (string query)
    {
        try
        {
            IsBusy = true;

            var images = await imageInfoService.SearchImages(query);
            if (ImageFiles.Count != 0)
                ImageFiles.Clear();

            foreach (var img in images)
            {
                ImageFiles.Add(img);
            }

           await UpdateResultsInfo();

        }
        catch (Exception ex) 
        {
            Debug.WriteLine($"Unable to get images: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally 
        { 
            IsBusy = false;
            IsRefreshing = false;
        }

      
    }

    private async Task UpdateResultsInfo()
    {
   
        var totalRecs = await imageInfoService.GetImagesCount();
        var imageWord = (ImageFiles.Count + ImageFiles.Count == 1) ? "image" : "images";

        resultsInfo = "Showing " + ImageFiles.Count + " " + imageWord;//
        if (ImageFiles.Count != totalRecs)
        {
            ResultsInfo += " of " + totalRecs.ToString();
        }
                                                                      
        if (SelectedImageFiles?.Count > 0)
        {
            ResultsInfo += " Selected images : " + SelectedImageFiles.Count.ToString();
        }
    }

    [RelayCommand]//(CanExecute =nameof(CanAddFile))
    async Task AddImage()
    {
        var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/comics", ".jpg", ".png", ".svg" } },
                    { DevicePlatform.WinUI, new[] { ".jpg", ".png",".svg" } },
            
                });

        PickOptions options = new()
        {
            PickerTitle = "Please select a image file",
            FileTypes= customFileType//PickOptions.Images.FileTypes //
        };

        try
        {
           // IsBusy = true;
            ImagesDatabase db = await ImagesDatabase.Instance;
            var multiResults = await FilePicker.Default.PickMultipleAsync(options);
            if (multiResults.Any())
            {
                if (multiResults.Count() ==1)
                {
                    var result = multiResults.FirstOrDefault();

                    var fi = new System.IO.FileInfo(result.FullPath);
                    var nameMinusExt = fi.Name.Replace(fi.Extension, "").TrimEnd('.');
                    var existingImage = await db.GetImageItemByNameAsync(nameMinusExt);
                    if (existingImage != null)
                    {
                        await Shell.Current.DisplayAlert("Duplicate image name!"
                            , "Please confirm that you haven't tried to add a file name that already exists in the library."
                            , "OK");
                        return;
                    }
                    var addedImg = await imageInfoService.AddImage(nameMinusExt, result.FullPath, "");
                    //TODO:  Check if we already have this exact image path.
                    if (addedImg != null)
                    {

                        await GoToDetails(addedImg);
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Failed to add image!"
                            , "Please confirm that you haven't tried to add a file name that already exists in the library, and the the selected file name is valid"
                            , "OK");
                    }
                }
                else//more than one file selected
                {
                    var hadUpdates = false;
                    foreach (FileResult result in multiResults)
                    {
                        var fi = new System.IO.FileInfo(result.FullPath);
                        var nameMinusExt = fi.Name.Replace(fi.Extension, "").TrimEnd('.');
                        var existingImage = await db.GetImageItemByNameAsync(nameMinusExt);
                        if (existingImage != null)
                        {
                            await Shell.Current.DisplayAlert("Duplicate image name : " + nameMinusExt
                                , "Please confirm that you haven't tried to add a file name that already exists in the library."
                                , "OK");
                            //return;
                        }
                        var addedImg = await imageInfoService.AddImage(nameMinusExt, result.FullPath, "");
                        //TODO:  Check if we already have this exact image path.
                        if (addedImg != null)
                        {
                            hadUpdates = true;
                            this.ImageFiles.Add(addedImg);
                           
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert("Failed to add image!"
                                , "Please confirm that you haven't tried to add a file name that already exists in the library, and the the selected file name is valid"
                                , "OK");
                        }
                    }

                    if (hadUpdates)
                    {
                        //Refresh the current listing?
                      await GetImagesAsync();
                        IsBusy = false;

                    }
                }
            }// any filepicker FileResult records
 
        }
        catch (Exception ex)
        {
            //App.Current.alert.
            Debug.WriteLine(ex.Message);
            //throw;
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy=false;
            IsRefreshing = false;


        }

    }

    [RelayCommand]
    async Task DeleteSelectedImages()
    {
        try
        {
            if (this.SelectedImageFiles.Count ==0)
            {
                return;
            }
            ImagesDatabase db = await ImagesDatabase.Instance;
            int totalDeleted = 0;
            int requestedFileDeleteCount = this.SelectedImageFiles.Count;
            var selectedCopy = SelectedImageFiles.ToList();
            foreach (var obj in selectedCopy)
            {
                var thisObj = (ImageFileInfo)obj;
                var sourceFilePath = thisObj.Location;
                var recsRemovedFromDb = await db.DeleteImageItemAsync(thisObj);

                if (recsRemovedFromDb > 0)
                {
                    //Delete this file from the file system
                    if (System.IO.File.Exists(sourceFilePath))
                    {
                        System.IO.File.Delete(sourceFilePath);

                        if (!System.IO.File.Exists(sourceFilePath))
                        {
                            totalDeleted += 1;
                            this.ImageFiles.Remove(thisObj);

                            if (sourceFilePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                            {
                                //Remove the supporting png file
                                if (System.IO.File.Exists(thisObj.Image))
                                {
                                    System.IO.File.Delete(thisObj.Image);
                                }
                            }
                        }
                    }

                    this.ImageFiles.Remove(thisObj);
                    SelectedImageFiles.Remove(thisObj);
                }

            }

            if (totalDeleted != requestedFileDeleteCount)
            {
                await Shell.Current.DisplayAlert("Could not delete all files","Please verify that the selected files exist in your file system.", "OK");
            }
            else
            {
                //Refresh the current listing?
              await GetImagesAsync();

            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            //throw;
            await Shell.Current.DisplayAlert("Error trying to delete files", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task RemoveSelectedImages()
    {
        try
        {
            if (this.SelectedImageFiles.Count == 0)
            {
                return;
            }
            ImagesDatabase db = await ImagesDatabase.Instance;
            int totalDeleted = 0;
            int requestedFileDeleteCount = this.SelectedImageFiles.Count;
            var selectedCopy = SelectedImageFiles.ToList();
            foreach (var obj in selectedCopy)
            {
                var thisObj = (ImageFileInfo)obj;
                var sourceFilePath = thisObj.Location;
                var recsRemovedFromDb = await db.DeleteImageItemAsync(thisObj);

                if (recsRemovedFromDb > 0)
                {
                    totalDeleted += 1;
                }

                this.ImageFiles.Remove(thisObj);
                SelectedImageFiles.Remove(thisObj);
            }

            if (totalDeleted != requestedFileDeleteCount)
            {
                
                await Shell.Current.DisplayAlert("Could not delete all files", "Please verify that the selected files exist in your file system.", "OK");
            }
            else
            {
                //Refresh the current listing?
                //await GetImagesAsync();
                SelectedImageFiles.Clear();
                IsRefreshing = false;

            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            //throw;
            await Shell.Current.DisplayAlert("Error trying to delete files", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task DeleteImage (ImageFileInfo imgToDelete)
    {
        try
        {
            //if (this.SelectedImageFiles.Count == 0)
            //{
            //    return;
            //}
            ImagesDatabase db = await ImagesDatabase.Instance;
            int totalDeleted = 0;
            
            
           
            {
                
                var sourceFilePath = imgToDelete.Location;
                var recsRemovedFromDb = await db.DeleteImageItemAsync(imgToDelete);
                var existingImg = (from i in ImageFiles where i.ID == imgToDelete.ID select i).FirstOrDefault();
                if (existingImg != null)
                {
                    var wasRemoved = ImageFiles.Remove(existingImg);
                }
                if (recsRemovedFromDb > 0)
                {
                    totalDeleted++;
                    //Delete this file from the file system
                    if (System.IO.File.Exists(sourceFilePath))
                    {
                        System.IO.File.Delete(sourceFilePath);

                        if (!System.IO.File.Exists(sourceFilePath))
                        {
                            totalDeleted += 1;
                            
                               
                           
                            if (sourceFilePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                            {
                                //Remove the supporting png file
                                if (System.IO.File.Exists(imgToDelete.Image))
                                {
                                    System.IO.File.Delete(imgToDelete.Image);
                                }
                            }
                        }
                    }

                    
                   
                  //  OnPropertyChanged("ImageFiles");
                    if (SelectedImageFiles.Contains(imgToDelete))
                    {
                        SelectedImageFiles.Remove(imgToDelete);
                    }

                    if (Shell.Current.CurrentPage.GetType() == typeof(DetailsPage))
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                }

            }

            if (totalDeleted ==0)
            {
                await Shell.Current.DisplayAlert($"Could not delete {imgToDelete.Name}", "Please verify that the selected files exist in your file system.", "OK");
            }
            else
            {
                //Refresh the current listing?
                //SearchImages
               // await GetImagesAsync();

            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            //throw;
            await Shell.Current.DisplayAlert($"Error trying to delete file {imgToDelete.Name}", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
     private void  SelectionHasChanged(Object param)//object sender, SelectionChangedEventArgs e
    {
      
        this.SelectionCount = SelectedImageFiles.Count;
        resultsInfo = "Selected images: " + this.selectionCount.ToString();
        //await UpdateResultsInfo();
    }
     
    [RelayCommand]
    async Task GoToDetails(ImageFileInfo imageFile)
    {
        if (imageFile == null)
            return;

        var loadedImageInfo =await imageInfoService.GetFullyLoadedImageFromId(imageFile.ID);

       await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
        {
            {"FullyLoadedImage", loadedImageInfo }
        });


    }

    [RelayCommand]
    async Task GoToDetailsFromId(int imageFileId)
    {
        if (imageFileId == 0)
            return;

        var loadedImageInfo = await imageInfoService.GetFullyLoadedImageFromId(imageFileId);

        await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
        {
            {"FullyLoadedImage", loadedImageInfo }
        });


    }

    private bool CanAddFile()
    {
        return true;
    }

    void IRecipient<DeletedImageMessage>.Receive(DeletedImageMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var imgToDelete = message.Value;// m.Value;
            await DeleteImage(imgToDelete);
        });
    }

    void IRecipient<ChangedImageMessage>.Receive(ChangedImageMessage message)
    {
        MainThread.BeginInvokeOnMainThread( () =>
        {
            var changedImage = message.Value;
            var imgInCollection = (from i in this.ImageFiles where i.ID== changedImage.ID select i).FirstOrDefault();

            if (imgInCollection !=null)
            {
                ImageFiles.Remove(imgInCollection);
                
            }

            ImageFiles.Add(changedImage);

        });
    }
}
