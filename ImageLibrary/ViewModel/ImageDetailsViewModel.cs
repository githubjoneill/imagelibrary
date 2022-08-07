
using CommunityToolkit.Maui.Core.Extensions;
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
 

    public ImageDetailsViewModel(ImageInfoService infoService, TagService tagsService)
    {
        var abc = infoService.GetType();
        this.imageInfoService = infoService;
        this.tagService = tagsService;
        
        //this.Tags = new ObservableCollection<Tag>();
        //this.availableTags = new ObservableCollection<Tag>();

        //Task.Run(() => GetTagsAsync());
    }


    [RelayCommand]
    async Task EditDetails()
    {
        try
        {
            var abc = fullyLoadedImage.ImageFileInfo.Name;
            //await map.OpenAsync(Monkey.Latitude, Monkey.Longitude, new MapLaunchOptions
            //{
            //    Name = Monkey.Name,
            //    NavigationMode = NavigationMode.None
            //});
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to load image information {ex.Message}");
            await Shell.Current.DisplayAlert("Error, no Maps app!", ex.Message, "OK");
        }
    }

    [RelayCommand]
    async Task SaveChanges()
    {
        try
        {
            var img = fullyLoadedImage.ImageFileInfo;
           var updatedId = await imageInfoService.UpdateImage(img);

            var existingTagIds =await imageInfoService.GetImageTagIdsForImageId(fullyLoadedImage.ImageFileInfo.ID);

            if (Tags?.Count > 0)//fullyLoadedImage.Tags?.Count
            {
                foreach (var thisTag in Tags)//fullyLoadedImage.
                {
                    var matchExisting = (from t in existingTagIds where t.TagId == thisTag.ID select t).FirstOrDefault();
                    if (matchExisting is null)
                    {
                        ImageTag newImgTag = new ImageTag() { ImageId = fullyLoadedImage.ImageFileInfo.ID, TagId = thisTag.ID };
                      var idUpdated = await imageInfoService.AddImageTagAsync(newImgTag);
                    }
                }
            }

            //Check for removed tags
            foreach (ImageTag existingItem in existingTagIds)
            {
                var matchExisting = (from t in Tags where t.ID == existingItem.TagId select t).FirstOrDefault();//fullyLoadedImage.
                if (matchExisting is null)
                {
                    //Remove from db.
                    var idDeleted = await imageInfoService.RemoveImageTagAsync(existingItem);
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
       // await Task.Delay(1);
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
}
