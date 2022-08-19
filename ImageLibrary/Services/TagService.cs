using SkiaSharp;
using Svg.Skia;
using ImageLibrary.Data;
//using static Android.Graphics.ImageDecoder;


namespace ImageLibrary.Services;


public class TagService
{
    List<Tag> tagsList = new();
    //string _dbPath;


    public async Task<List<Tag>> GetTags()
    {
        

        try
        {
            if (tagsList?.Count > 0)
            {
                return tagsList;
            }
        }
        catch (Exception )
        {

            //throw;
        }
        var dataDir = FileSystem.AppDataDirectory;

        ImagesDatabase database = await ImagesDatabase.Instance;
        

        var allRecs = await database.GetAllTagAsync();

        var tagAssignments = await database.GetImageTagsAllAsync();


        if (tagAssignments ==null)
        {
            foreach (var tag in allRecs)
            {  
                tagsList.Add(tag);
            }
        }
        else
        {
            foreach (var tag in allRecs)
            {
                var instances = (from t in tagAssignments where t.TagId == tag.ID select t.ID).Count();
                tag.InstancesCount = instances;
                tagsList.Add(tag);
            }
        }
        

       

        return tagsList?? new List<Tag>();

    }

    public async Task<List<ImageTag>> GetImageTagsForImageId(int imageId)
    {

        ImagesDatabase database = await ImagesDatabase.Instance;
        var tagsForImageId = await database.GetImageTagsFromImageIdAsync(imageId);

        return tagsForImageId ?? new List<ImageTag>();
    }

    public async Task<List<ImageFileInfo>> GetImagesForTag(Tag tag)
    {
        ImagesDatabase db = await ImagesDatabase.Instance;

        return  await db.GetImageItemByTagAsync(tag);
        
    }

    public async Task<List<ImageTag>> GetImageTagsForTagId(int tagId)
    {

        ImagesDatabase database = await ImagesDatabase.Instance;
        var tagsForImageId = await database.GetImageTagsFromTagIdAsync(tagId);

        return tagsForImageId ?? new List<ImageTag>();
    }

    public async Task<Tag> AddTag(string tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return null;
        }

        Tag tagObj = new Tag() { Name = tag, InstancesCount =0 };

        ImagesDatabase db = await ImagesDatabase.Instance;
       var rec = await db.SaveTagItemAsync(tagObj);

        if (tagObj.ID <1)
        {
            var tagLookup =await db.GetTagItemByNameAsync(tag);
            if (tagLookup !=null)
            {
                tagObj.ID = tagLookup.ID;//rec;
            }
        }
        //Belt and braces check if the ID value wasn't set.
        
       
        return tagObj;// reply;
    }

    public async Task<int> DeleteTag(string tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return 0;
        }


        int recsUpdated = 0;
        ImagesDatabase db = await ImagesDatabase.Instance;
        var allRecs = await db.GetAllTagAsync();
        var item = (from a in allRecs where a.Name.ToLower() == tag.ToLower() select a).FirstOrDefault();

        if (item != null)
        {
            var tagsForImageId = await db.GetImageTagsFromTagIdAsync(item.ID);

            if (tagsForImageId != null)
            {
                foreach (var tagItem in tagsForImageId)
                {
                    var idDeleted = await db.DeleteImageTagItemAsync(tagItem);
                } 
                
            }
            recsUpdated = await db.DeleteTagItemAsync(item);
        }
       

        return recsUpdated;// reply;
    }


}
