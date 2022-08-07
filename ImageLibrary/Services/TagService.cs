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
        
        //Tag defaultTag =new Tag();
        //defaultTag.Name = "Food";
        //await database.SaveTagItemAsync(defaultTag);
        var allRecs = await database.GetAllTagAsync();

        foreach (var tag in allRecs)
        {
            tagsList.Add(tag);
        }

        //tagsList.Sort();

        return tagsList?? new List<Tag>();

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
        tagObj.ID = rec;
        return tagObj;// reply;
    }

    public async Task<int> DeleteTag(string tag)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return 0;
        }

        

        ImagesDatabase db = await ImagesDatabase.Instance;
        var allRecs = await db.GetAllTagAsync();
        var item = (from a in allRecs where a.Name.ToLower() == tag.ToLower() select a).FirstOrDefault();
        var recsUpdated = await db.DeleteTagItemAsync(item);

        return recsUpdated;// reply;
    }


}
