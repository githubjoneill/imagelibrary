
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using ImageLibrary.Messages;
using ImageLibrary.Services;
using ImageLibrary.View;

namespace ImageLibrary.ViewModel;

[QueryProperty(nameof(FullyLoadedImage), "FullyLoadedImage")]

public partial class ImageDetailsViewModel: BaseViewModel
{
    TagService tagService;
    ImageInfoService imageInfoService;


    //ObservableCollection<Tag> Tags { get; } = new();
    [ObservableProperty]
    ObservableCollection<Tag> tags;

    [ObservableProperty]
    ObservableCollection<Tag> availableTags;


    [ObservableProperty]
    public FullyLoadedImageObject fullyLoadedImage;

    [ObservableProperty]
    string tagAddedText;
 

    public ImageDetailsViewModel(ImageInfoService infoService, TagService tagsService)
    {
       // var abc = infoService.GetType();
        this.imageInfoService = infoService;
        this.tagService = tagsService;
    }


    [RelayCommand]
    async Task Delete()
    {
        try
        {
           // var abc = fullyLoadedImage.ImageFileInfo.Name;
            WeakReferenceMessenger.Default.Send(new DeletedImageMessage(fullyLoadedImage.ImageFileInfo));  
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to delete image information {ex.Message}");
            await Shell.Current.DisplayAlert("Error, cannot delete!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task SaveChanges()
    {
        try
        {
            var img = fullyLoadedImage.ImageFileInfo;
           var updatedId = await imageInfoService.UpdateImage(img);
            WeakReferenceMessenger.Default.Send(new ChangedImageMessage(fullyLoadedImage.ImageFileInfo));

            var existingTagIds =await imageInfoService.GetImageTagIdsForImageId(fullyLoadedImage.ImageFileInfo.ID);

            if (Tags?.Count > 0)//fullyLoadedImage.Tags?.Count
            {
                foreach (var thisTag in Tags.Where(t => t !=null))//fullyLoadedImage.
                {
                    ImageTag matchExisting = null;
                    if (existingTagIds?.Count >0)
                    {
                        matchExisting = (from t in existingTagIds where t.TagId == thisTag.ID select t).FirstOrDefault();
                    }
                    
                    if (matchExisting is null)
                    {
                        ImageTag newImgTag = new ImageTag() { ImageId = fullyLoadedImage.ImageFileInfo.ID, TagId = thisTag.ID };
                      var idUpdated = await imageInfoService.AddImageTagAsync(newImgTag);
                        if (idUpdated >0)
                        {
                            WeakReferenceMessenger.Default.Send(new ChangedImageTagMessage(newImgTag));
                        }
                    }
                }
            }

            //Check for removed tags
            if (existingTagIds !=null)
            {
                foreach (ImageTag existingItem in existingTagIds)
                {
                    var matchExisting = (from t in Tags where t!=null && t.ID == existingItem.TagId select t).FirstOrDefault();//fullyLoadedImage.
                    if (matchExisting is null)
                    {
                        //Remove from db.
                        var idDeleted = await imageInfoService.RemoveImageTagAsync(existingItem);
                        if (idDeleted > 0)
                        {
                            WeakReferenceMessenger.Default.Send(new DeletedImageTagMessage(existingItem));
                        }

                    }
                }
            }
          
            if (updatedId >0)
            {
                await Shell.Current.GoToAsync("..");
            }
            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to load image information {ex.Message}");
            await Shell.Current.DisplayAlert("Error, no Maps app!", ex.Message, "OK");

        }
    }

    async Task GetTagsAsync()
    {
        if (fullyLoadedImage is null)
        {
            return;
        }
        if (IsBusy )
            return;
        try
        {

            IsBusy = true;
            var allTags = await tagService.GetTags();
            allTags = allTags.OrderBy(a => a.Name).ToList();
            if (Tags?.Count != 0)//FullyLoadedImage.Tags?.Count != 0
                Tags.Clear();

            foreach (var tagItem in allTags)
                Tags.Add(tagItem);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get tags: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
           // IsRefreshing = false;
        }
    }

    [RelayCommand]
     void AddTag(Tag tag)//async Task
    {
       
        Tags.Add(tag);//

        AvailableTags.Remove(tag);
    }

    [RelayCommand]
     void RemoveTag(Tag tag)
    {
        
        AvailableTags.Add(tag);
        AvailableTags = AvailableTags.OrderBy(a => a.Name).ToObservableCollection();

        Tags.Remove(tag);
    }

    [RelayCommand]
    void CancelGoHome ()
    {
        Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    async Task EnterNewTagText (string newTagText)
    {
        if (string.IsNullOrEmpty(this.TagAddedText))
        {
            return;
        }

        this.IsBusy = true;

        try
        {
            var matchedTag = (from t in AvailableTags where t.Name.Equals(TagAddedText, StringComparison.OrdinalIgnoreCase) select t).FirstOrDefault();
            if (matchedTag == null)
            {
                // Create a new tag and save it to db /local observable collection.
               var thisNewTag =  await tagService.AddTag(newTagText);
               if (thisNewTag != null && thisNewTag.ID > 0)
                {
                    Tags.Add(thisNewTag);
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
}
