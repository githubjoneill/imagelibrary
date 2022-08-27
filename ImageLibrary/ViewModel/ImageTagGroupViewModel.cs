using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using ImageLibrary.Data;
using ImageLibrary.Messages;
using ImageLibrary.Services;
using ImageLibrary.View;


namespace ImageLibrary.ViewModel;


public partial class ImageTagGroupViewModel : BaseViewModel, IRecipient<DeletedImageMessage>, IRecipient<ChangedImageMessage>, IRecipient<ChangedImageTagMessage>

{
    internal ImageInfoService imageInfoService;
    internal TagService tagService;
    IConnectivity connectivity;

    [ObservableProperty]
    ObservableCollection<Tag> tagsAssigned;

    [ObservableProperty]
    ObservableCollection<Tag> tagsAvailable;

    [ObservableProperty]
    ObservableCollection<TagImageGroup> imageTagGroups;


   

    public ImageTagGroupViewModel(ImageInfoService infoService, TagService tagSvc, IConnectivity connectivity)
    {
        Title = "Image listing by tag";
        this.imageInfoService = infoService;
        this.tagService = tagSvc;
        this.connectivity = connectivity;

        Task.Run(() => LoadTagGroupsAsync());

        WeakReferenceMessenger.Default.Register<DeletedImageMessage>(this);
        WeakReferenceMessenger.Default.Register<ChangedImageMessage>(this);
        WeakReferenceMessenger.Default.Register<ChangedImageTagMessage>(this);

    }


    [RelayCommand]
    async Task RefreshImages()
    {
        IsRefreshing = true;
        IsBusy = true;
        tagService.ClearTagCache();
        await LoadTagGroupsAsync();
    }

    private async Task LoadTagGroupsAsync()
    {
        //IsRefreshing = true;
        IsBusy = true;
        try
        {
            if (ImageTagGroups is null)
            {
                ImageTagGroups = new ObservableCollection<TagImageGroup>();
            }
            ImageTagGroups.Clear();

            var lstFiles = await imageInfoService.GetUntaggedImages();
            if (lstFiles?.Count > 0)
            {
                var grp = new TagImageGroup("Untagged", lstFiles);
                ImageTagGroups.Add(grp);
            }


            var allTags = await tagService.GetTags();
            if (allTags?.Count > 0)
            {
                foreach (var tag in allTags.ToArray())
                {
                    var images = await tagService.GetImagesForTag(tag);

                    if (images?.Count > 0)
                    {
                        ImageTagGroups.Add(new TagImageGroup(tag.Name, images));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get images and their tags: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
           // OnPropertyChanged(nameof(ImageTagGroups));
            IsBusy = false;
            IsRefreshing = false;
        }
    }

   
    public List<Object> SelectedImageFiles { get; set; } = new();


    [ObservableProperty]
    bool isRefreshing;

    [NotifyPropertyChangedFor(nameof(ResultsInfo))]
    [ObservableProperty]
    int selectionCount;

    [ObservableProperty]
    string resultsInfo;

    
    [ObservableProperty]
    TagMultiAssignments tagMultiAssignments;

    [RelayCommand]
    private void SelectionHasChanged(Object param)//object sender, SelectionChangedEventArgs e
    {

        this.SelectionCount = SelectedImageFiles.Count;
        resultsInfo = "Selected images: " + this.selectionCount.ToString();
      
    }

    [RelayCommand]
    async Task GoToDetails(ImageFileInfo imageFile)
    {
        if (imageFile == null)
            return;

        var loadedImageInfo = await imageInfoService.GetFullyLoadedImageFromId(imageFile.ID);

        await Shell.Current.GoToAsync(nameof(DetailsPage), true, new Dictionary<string, object>
        {
            {"FullyLoadedImage", loadedImageInfo }
        });


    }

    [RelayCommand]
    async Task TagSelectedImages()
    {
        if (this.SelectedImageFiles is null || SelectedImageFiles.Count == 0)
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
    void CancelGoHome()
    {
        Shell.Current.GoToAsync("..");
    }

    void IRecipient<DeletedImageMessage>.Receive(DeletedImageMessage message)
    {
        MainThread.BeginInvokeOnMainThread( async () =>//async () =>
        {
            var imgToDelete = message.Value;
            await LoadTagGroupsAsync();
           // await DeleteImage(imgToDelete);
        });
    }

    void IRecipient<ChangedImageMessage>.Receive(ChangedImageMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var changedImage = message.Value;
            //await LoadTagGroupsAsync();
           
            //OnPropertyChanged(nameof(ImageTagGroups));


        });
    }

    public void Receive(ChangedImageTagMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            //var changedImageTag = message.Value;
            //var allTags = await tagService.GetTags();
            //if (allTags?.Count > 0)
            //{
            //    var selectedTag = (from a in allTags where a.ID == changedImageTag.TagId select a).FirstOrDefault();
            //    //First check in the untagged group for an imageId that generated this imageTag change
            //    var allUntaggedImages = (from i in imageTagGroups where i.Name.Equals("Untagged", StringComparison.OrdinalIgnoreCase) select i.ToList()).FirstOrDefault();
            //    if (allUntaggedImages !=null)
            //    {
            //        var matchedUntagged = (from i in allUntaggedImages where i.ID  == changedImageTag.ImageId select i).FirstOrDefault();
            //        if (matchedUntagged !=null )
            //        {
            //            ImageTagGroups[0].Remove(matchedUntagged);
            //        }
            //    }
              

            //    //Check each tag in allTags for one matching this changed imagTag
            //    foreach (var tagGroup in ImageTagGroups)
            //    {
            //        if (tagGroup.Name.Equals(selectedTag.Name, StringComparison.OrdinalIgnoreCase))
            //        {
            //            var matchedChild = (from g in tagGroup where g.ID == changedImageTag.ID select g).FirstOrDefault();
            //            if (matchedChild is null)
            //            {
            //                var img = await imageInfoService.GetImageFromImageId(changedImageTag.ImageId);
            //                if (img !=null)
            //                {
            //                    tagGroup.Add(img);
            //                }
            //            }
            //        }
            //    }
            //}

               await LoadTagGroupsAsync();
              
                OnPropertyChanged(nameof(ImageTagGroups));


        });
    }
}

