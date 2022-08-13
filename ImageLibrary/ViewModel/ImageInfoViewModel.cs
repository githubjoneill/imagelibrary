using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using ImageLibrary.Data;
using ImageLibrary.Messages;
using ImageLibrary.Model;
using ImageLibrary.Services;
using ImageLibrary.View;

//using Java.Security;
//using ImageLibrary.View;

namespace ImageLibrary.ViewModel;

[QueryProperty(nameof(TagMultiAssignments), "TagMultiAssignments")]

public partial class ImageInfoViewModel: BaseViewModel, IRecipient<DeletedImageMessage>, IRecipient<ChangedImageMessage>
{

  

    public ObservableCollection<ImageFileInfo> ImageFiles { get; } = new();

    //public ObservableCollection<ImageFileInfo> UntaggedImageFiles { get; } = new();
    [ObservableProperty]
    ObservableCollection<ImageFileInfo> untaggedImageFiles;

    [ObservableProperty]
    ObservableCollection<Tag> tagsAssigned;

    [ObservableProperty]
    ObservableCollection<Tag> tagsAvailable;



    public List<Object> SelectedImageFiles { get; set; } = new();

    ImageInfoService imageInfoService;
    TagService tagService;
    IConnectivity connectivity;

    public ImageInfoViewModel(ImageInfoService infoService, TagService tagSvc, IConnectivity connectivity)
    {
        Title = "Image Library";
        this.imageInfoService = infoService;
        this.tagService = tagSvc;
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

    [ObservableProperty]
    string tagAddedText;

    [ObservableProperty]
    TagMultiAssignments tagMultiAssignments;

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
    async Task TagSelectedImages()
    {
        if (this.SelectedImageFiles is null || SelectedImageFiles.Count ==0)
        {
            return;
        }
        try
        {
            TagsAvailable = new ObservableCollection<Tag>();
            TagsAssigned = new ObservableCollection<Tag>();

            var tags = await tagService.GetTags();
            if (tags?.Count > 0)
            {
                foreach (var tag in tags.Where(t => t != null))
                {
                    TagsAvailable.Add(tag);
                }
            }

            // Logic to move tags from AvailableTags list to assigned Tags list if any tags are present in ALL selected files
            List<int> commonTagIds = new();
            List<ImageTag> allAssignedImageTags = new();
            foreach (var obj in SelectedImageFiles)
            {
                ImageFileInfo img = obj as ImageFileInfo;
                var tagsForImage = await imageInfoService.GetImageTagIdsForImageId(img.ID);
                if (tagsForImage != null && tagsForImage.Count > 0)
                {
                    allAssignedImageTags.AddRange(tagsForImage);

                    var tagids = (from t in tagsForImage select t.TagId).ToList();
                    commonTagIds.AddRange(tagids.Where(x => !commonTagIds.Contains(x)));
                }
            }

            //loop through all distinct tagIds.  If any apply to all selected files, remove that tag from the TagsAvailable list and add to the TagsAssigned list.
            if (commonTagIds?.Count > 0)
            {
                foreach (var tagId in commonTagIds)
                {
                    var filesCountWithThisTag = (from f in allAssignedImageTags where f.TagId == tagId select f).Count();
                    if (filesCountWithThisTag == SelectedImageFiles.Count)
                    {
                        var commonTag = (from t in TagsAvailable where t.ID == tagId select t).FirstOrDefault();
                        if (TagsAssigned is null)
                        {
                            TagsAssigned = new ObservableCollection<Tag>();
                        }
                        TagsAssigned.Add(commonTag);
                        TagsAvailable.Remove(commonTag);
                    }
                }
            }

            TagMultiAssignments tma = new TagMultiAssignments();
            foreach (var obj in SelectedImageFiles)
            {
                ImageFileInfo img = obj as ImageFileInfo;
                tma.images.Add(img);
            }
            tma.tagsAssigned = TagsAssigned.ToList();
            tma.tagsAvailable = TagsAvailable.ToList();

            // End of block to move any availableTags to assigned tags
            await Shell.Current.GoToAsync(nameof(TagsAssignPage), true, new Dictionary<string, object>
                {
                    {"TagMultiAssignments", tma }
                });
        }
        catch (Exception ex)
        {
           await Shell.Current.DisplayAlert("Can't assign tags", ex.Message, "OK");
        }
       
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

    [RelayCommand]
    async Task EnterNewTagText(string newTagText)
    {
        if (string.IsNullOrEmpty(this.TagAddedText))
        {
            return;
        }

        this.IsBusy = true;

        try
        {
            if (TagsAvailable is null)
            {
                TagsAvailable = new ObservableCollection<Tag>();
            }
            if (TagsAssigned is null)
            {
                TagsAssigned = new ObservableCollection<Tag>();
            }
            var matchedTag = (from t in TagsAvailable where t.Name.Equals(TagAddedText, StringComparison.OrdinalIgnoreCase) select t).FirstOrDefault();
            if (matchedTag == null)
            {
                // Create a new tag and save it to db /local observable collection.
                var thisNewTag = await tagService.AddTag(newTagText);
                if (thisNewTag != null && thisNewTag.ID > 0)
                {
                    TagsAssigned.Add(thisNewTag);
                }
            }
            else
            {
                AddTag(matchedTag);
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            TagAddedText = null;
            this.IsBusy = false;
        }
    }

    [RelayCommand]
    void AddTag(Tag tag)//async Task
    {
        // await Task.Delay(1);
       TagsAssigned.Add(tag);//

        TagsAvailable.Remove(tag);
    }

    [RelayCommand]
    void RemoveTag(Tag tag)
    {
        TagsAvailable.Add(tag);
        TagsAvailable = TagsAvailable.OrderBy(a => a.Name).ToObservableCollection();

        TagsAssigned.Remove(tag);
    }

    [RelayCommand]
    async Task SaveSelctedTagChanges()
    {
        try
        {
            var loadedObj = this.tagMultiAssignments;
            if (loadedObj==null || loadedObj.images?.Count ==0)
            {
                return;
            }

            foreach (var imageFile in loadedObj.images)
            {
                var existingTagIds = await imageInfoService.GetImageTagIdsForImageId(imageFile.ID);
                if (TagsAssigned?.Count >0)
                {
                    foreach (var thisTag in TagsAssigned.Where(t=> t !=null))
                    {
                        var matchExisting = (from e in existingTagIds where e.TagId == thisTag.ID select e).FirstOrDefault();
                        if (matchExisting is null)
                        {
                            ImageTag newImgTag = new ImageTag() { ImageId = imageFile.ID, TagId = thisTag.ID };
                            var idUpdated = await imageInfoService.AddImageTagAsync(newImgTag);
                        }
                    }
                }
            }
            //if (this.TagsAssigned?.Count > 0)
            //{

            //}
            //var img = fullyLoadedImage.ImageFileInfo;
            //var updatedId = await imageInfoService.UpdateImage(img);
            //WeakReferenceMessenger.Default.Send(new ChangedImageMessage(fullyLoadedImage.ImageFileInfo));

            //var existingTagIds = await imageInfoService.GetImageTagIdsForImageId(fullyLoadedImage.ImageFileInfo.ID);

            //if (Tags?.Count > 0)//fullyLoadedImage.Tags?.Count
            //{
            //    foreach (var thisTag in Tags.Where(t => t != null))//fullyLoadedImage.
            //    {
            //        ImageTag matchExisting = null;
            //        if (existingTagIds?.Count > 0)
            //        {
            //            matchExisting = (from t in existingTagIds where t.TagId == thisTag.ID select t).FirstOrDefault();
            //        }

            //        if (matchExisting is null)
            //        {
            //            ImageTag newImgTag = new ImageTag() { ImageId = fullyLoadedImage.ImageFileInfo.ID, TagId = thisTag.ID };
            //            var idUpdated = await imageInfoService.AddImageTagAsync(newImgTag);
            //        }
            //    }
            //}

            ////Check for removed tags
            //if (existingTagIds != null)
            //{
            //    foreach (ImageTag existingItem in existingTagIds)
            //    {
            //        var matchExisting = (from t in Tags where t != null && t.ID == existingItem.TagId select t).FirstOrDefault();//fullyLoadedImage.
            //        if (matchExisting is null)
            //        {
            //            //Remove from db.
            //            var idDeleted = await imageInfoService.RemoveImageTagAsync(existingItem);
            //        }
            //    }
            //}

            //if (updatedId > 0)
            //{
         await Shell.Current.GoToAsync("..");
            //}

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to load image information {ex.Message}");
            await Shell.Current.DisplayAlert("Error, no Maps app!", ex.Message, "OK");

        }
    }

    [RelayCommand]
    void CancelGoHome()
    {
        Shell.Current.GoToAsync("..");
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

            //imgInCollection = changedImage;
            //OnPropertyChanged(nameof(imgInCollection));
            //OnPropertyChanged(nameof(ImageFiles));
            if (imgInCollection != null)
            {
                ImageFiles.Remove(imgInCollection);

            }

            ImageFiles.Add(changedImage);

        });
    }
}
