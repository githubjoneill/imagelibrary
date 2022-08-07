using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Model;


public partial class FullyLoadedImageObject : ObservableObject
{

    public FullyLoadedImageObject(ImageFileInfo seed, List<Tag> availableTags, List<Tag> assignedTags)
    {
        this.ImageFileInfo = seed;
        this.AvailableTags = new ObservableCollection<Tag>(availableTags);
        this.AssignedTags = new ObservableCollection<Tag>(assignedTags);
        //this.AvailableTags = availableTags;
        //this.Tags = assignedTags;
    }

    [ObservableProperty]
    private ImageFileInfo imageFileInfo;//{ get; init ; }


    //public ObservableCollection<Tag> AvailableTags { get; set; } = new ();


    [ObservableProperty]
    private ObservableCollection<Tag> assignedTags;

    [ObservableProperty]
    private ObservableCollection<Tag> availableTags = new ();

    //[ObservableProperty]
    //private ObservableCollection<Tag> tags;// { get; set; }

   // private List<ImageTag> _imageTags = new List<ImageTag>();

    public bool AddAvailableTag (Tag newTag)
    {
        bool reply = false;
        foreach (var existingTag in AvailableTags)
        {
            if (existingTag.Name.Equals(newTag.Name,StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        //Assume the new tag has been saved and has an Id
       // _imageTags.Add (new ImageTag() { ImageId= ImageFileInfo.ID, TagId = newTag.ID });
        AvailableTags.Add(newTag); 
        reply = true;

        return reply;
    }

    //public bool AddTag(Tag newTag)
    //{
    //    bool reply = false;
    //    foreach (var existingTag in Tags)
    //    {
    //        if (existingTag.Name.Equals(newTag.Name, StringComparison.OrdinalIgnoreCase))
    //        {
    //            return false;
    //        }
    //    }

    //    //Assume the new tag has been saved and has an Id
    //    // _imageTags.Add (new ImageTag() { ImageId= ImageFileInfo.ID, TagId = newTag.ID });
    //    Tags.Add(newTag);
    //    reply = true;

    //    return reply;
    //}

    public bool RemoveAssignedTag (Tag removedTag)
    {
        var existingTag = (from t in AssignedTags where t.Name.Equals(removedTag.Name, StringComparison.OrdinalIgnoreCase) select t).FirstOrDefault();

        if (existingTag !=null)
        {
            AssignedTags.Remove(existingTag);
            return true;
        }

        return false;
    }

    public bool RemoveAvailableTag(Tag removedTag)
    {
        var existingTag = (from t in AvailableTags where t.Name.Equals(removedTag.Name, StringComparison.OrdinalIgnoreCase) select t).FirstOrDefault();

        if (existingTag != null)
        {
            AvailableTags.Remove(existingTag);
            return true;
        }

        return false;
    }
}
