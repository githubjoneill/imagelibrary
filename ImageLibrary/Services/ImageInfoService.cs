using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http.Json;
using SkiaSharp;
using Svg.Skia;
using ImageLibrary.Data;
//using Kotlin.Contracts;
//using Svg.ExCSS;

namespace ImageLibrary.Services;

public class ImageInfoService
{

    HttpClient httpClient;
    List<ImageFileInfo> imagesList = new();


    public ImageInfoService()
	{
        this.httpClient = new HttpClient();
    }
     
    public async Task<List<ImageFileInfo>> GetImagesInfo()
    {
        if (imagesList?.Count >0)
        {
            return imagesList;
        }


        ////Online block
        //var response = await httpClient.GetAsync("myJsonUrlHere.json");
        //if (response.IsSuccessStatusCode)
        //{
        //    imagesList = await response.Content.ReadFromJsonAsync<List<ImageFileInfo>>();
        //}

        //Offline
        //using var stream = await FileSystem.OpenAppPackageFileAsync("AboutAssets.txt");
        //using var reader = new StreamReader(stream);

        //var contents = reader.ReadToEnd();
        var dataDir = FileSystem.AppDataDirectory;

        ImagesDatabase database = await ImagesDatabase.Instance;
        var allRecs = await database.GetImageItemsAsync();
        //if (allRecs ==null || allRecs.Count ==0)
        //{
        //    using var stream = await FileSystem.OpenAppPackageFileAsync("sampledata.json");
        //    using var reader = new StreamReader(stream);
        //    var contents = await reader.ReadToEndAsync();
        //    // imagesList = JsonSerializer.Deserialize<List<ImageFileInfo>>(contents);
        //    var sampleList = JsonSerializer.Deserialize<List<ImageFileInfo>>(contents);
        //    if (sampleList?.Count > 0)
        //    {
        //        imagesList = new();
        //        foreach (var item in sampleList)
        //        {
        //            AddImage(item.Name, item.Image, item.Description);

        //           var nextId = await  database.SaveImageItemAsync(item);
        //        }
        //    }
        //}
        //else//load from the database
        //{
            foreach (var item in allRecs)
            {
                imagesList.Add(item);
            }
       // }


       
        return imagesList;
    }

    public async Task< List<ImageFileInfo>> SearchImages(string searchTerm)
    {
        List<ImageFileInfo> resultList = new();
        var dataDir = FileSystem.AppDataDirectory;

        ImagesDatabase database = await ImagesDatabase.Instance;
        //Check the entire entry?  OR break up entry by spaces?

        if (string.IsNullOrEmpty(searchTerm))
        {
            //Get all rows
            resultList = await database.GetImageItemsAsync();
            //resultList = resultList.Take(50).ToList();
            imagesList.Clear();
            
            return resultList;
        }

        searchTerm = searchTerm.Trim();

        var allTags = await database.GetAllTagAsync();
        if (allTags is null)
        {
            allTags = new List<Tag>();
        }


        if (!searchTerm.Contains(" "))
        {
            resultList = await database.GetImageItemsByNameLikeAsync(searchTerm);

            var tagMatch = (from at in allTags where at.Name.Equals(searchTerm, StringComparison.CurrentCultureIgnoreCase) select at).FirstOrDefault();
            if (tagMatch !=null)
            {
                var tagResults = await database.GetImageItemByTagAsync(tagMatch);
                if (tagResults?.Count > 0)
                {
                    resultList.AddRange(tagResults);
                    resultList = resultList.Distinct().ToList();
                }
               
            }
        }

        if (searchTerm.Contains("+"))
        {
            var joinedElements = searchTerm.Replace(" ","").Split('+');
            var constructedQuery = string.Join(" ",joinedElements);
            var termRecs = await database.GetImageItemsByNameLikeAsync(constructedQuery);
            if (termRecs?.Count > 0)
            {
                resultList.AddRange(termRecs);
            }
              
        }
        else// No "+" characters found in search term, just split words by spaces and ignore terms like "and" and "with"
        {
            var searchElements = searchTerm.Split(' ');
            foreach (var term in searchElements)
            {
                if (!term.Equals("and", StringComparison.OrdinalIgnoreCase)
                    && !term.Equals("with", StringComparison.OrdinalIgnoreCase)
                    && !term.Equals(" ", StringComparison.OrdinalIgnoreCase))
                {
                    var termRecs = await database.GetImageItemsByNameLikeAsync(term);

                    //Check if the term is a tag. If so, get any images that have this tag.
                    if (termRecs?.Count > 0)
                    {
                        foreach (var termRec in termRecs)
                        {
                            var foundMatch = (from r in resultList where r.ID == termRec.ID select r).FirstOrDefault();
                            if (foundMatch is null)
                            {
                                resultList.Add(termRec);
                            }
                        }
                       
                    }
                }

            }
        }
    
        

        return resultList;

    }

    private  string SvgFileToPng(string svgFilePath)
    {
        string replyPath = string.Empty;
        Stream imageStream = null;
        try
        {
          

            if (string.IsNullOrEmpty(svgFilePath))
            {
                return replyPath;
            }

            if (!svgFilePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                if (!File.Exists(svgFilePath))
                {
                    return replyPath;
                }
            }
            //else
            //{
               
            //    Image image = new Image
            //    {
            //        Source = ImageSource.FromUri(new Uri(svgFilePath))
            //    };
              

            //}


            var fInfo = new System.IO.FileInfo(svgFilePath);
            var fName = fInfo.Name.Replace(fInfo.Extension, ".png");

            var dataDir = FileSystem.AppDataDirectory;
            dataDir = Path.Combine(dataDir, @"ConvertedSvgFiles");
            if (!System.IO.Directory.Exists(dataDir))
            {
                System.IO.Directory.CreateDirectory(dataDir);
            }
            string convertedFilePath = Path.Combine(dataDir, fName);

            if (System.IO.File.Exists(convertedFilePath))
            {
                return convertedFilePath;
            }

            using (var svg = new SKSvg())
            {
                if (svg.Load(svgFilePath) is { })
                {
                    bool wasSaved = false;
                    if (imageStream != null)
                    {
                        wasSaved = svg.Save(imageStream, SKColor.Empty, SKEncodedImageFormat.Png, 100, 1f, 1f);
                    }
                    else

                        wasSaved = svg.Save(convertedFilePath, SKColor.Empty, SKEncodedImageFormat.Png, 100, 1f, 1f);
                    if (wasSaved)
                    {
                        replyPath = convertedFilePath;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
       
        return replyPath;
    }

    public async Task<ImageFileInfo>  AddImage(string friendlyName, string location, string description )
    {
       
        if (string.IsNullOrEmpty(location))
        {
            return null;
        }
        string physicalLocaton = location;
        if (location.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
           physicalLocaton =  SvgFileToPng(location);

        }
        var fInfo = new System.IO.FileInfo(physicalLocaton);
        var imgInfo = new ImageFileInfo() { Description =description, FileName =fInfo.Name, Image = physicalLocaton, Name = friendlyName, Location = location };
        imagesList.Add(imgInfo);

        ImagesDatabase db = await ImagesDatabase.Instance;
        await db.SaveImageItemAsync(imgInfo);
        return imgInfo;
    }

    public async Task<int> UpdateImage (ImageFileInfo img)
    {

        ImagesDatabase database = await ImagesDatabase.Instance;
        int idUpdated = await   database.SaveImageItemAsync (img);
        return idUpdated;


    }
    public async Task<int> GetImagesCount()
    {
         ImagesDatabase database = await ImagesDatabase.Instance;

        return await database.GetTotalImagesCountAsync();
    }

    public async Task<List<ImageTag>> GetImageTagIdsForImageId(int imageId)
    {
        ImagesDatabase database = await ImagesDatabase.Instance;
        return await  database.GetImageTagsFromImageIdAsync(imageId);

    }

    public async Task<int> AddImageTagAsync (ImageTag imgTag)
    {
        ImagesDatabase database = await ImagesDatabase.Instance;
        var existingIds = await database.GetImageTagsFromImageIdAsync (imgTag.ImageId);
        var matchExisting = (from t in existingIds where t.ImageId == imgTag.ImageId && t.TagId == imgTag.TagId select t).FirstOrDefault();

        if (matchExisting != null)
        {
            return 0;
        }
        else
        {
           return await database.SaveImageTagItemAsync(imgTag);
            
        }
    }

    public async Task<int> RemoveImageTagAsync(ImageTag imgTag)
    {
        ImagesDatabase database = await ImagesDatabase.Instance;
        var existingIds = await database.GetImageTagsFromImageIdAsync(imgTag.ImageId);
        var matchExisting = (from t in existingIds where t.ImageId == imgTag.ImageId && t.TagId == imgTag.TagId select t).FirstOrDefault();

        if (matchExisting == null)
        {
            return 0;
        }
        else
        {
            return await database.DeleteImageTagItemAsync(imgTag);

        }
    }

    public async Task<FullyLoadedImageObject>GetFullyLoadedImageFromId (int id)
    {
        FullyLoadedImageObject imageObject = null;
        ImagesDatabase database = await ImagesDatabase.Instance;

        var imgOnly = await database.GetImageItemAsync(id);

        if (imgOnly ==null)
        { 
            return imageObject;
        }

        var allTags = await database.GetAllTagAsync();
        allTags = allTags.OrderBy(t => t.Name).ToList();
        List<Tag> assignedTags = new();
       // imageObject = new FullyLoadedImageObject(imgOnly, allTags);

        var tagIdsForThisImage = await database.GetImageTagsFromImageIdAsync(id);

        if (tagIdsForThisImage?.Count > 0)
        {
            foreach (var item in tagIdsForThisImage)
            {
                var thisTagId = item.TagId;
                var t = (from at in allTags where at.ID== thisTagId select at).FirstOrDefault();

                // imageObject.AddTag(t);
                assignedTags.Add(t);

                allTags.Remove(t);

                
            }
        }
       imageObject = new FullyLoadedImageObject(imgOnly,allTags,assignedTags);

        return imageObject;
    }

}
