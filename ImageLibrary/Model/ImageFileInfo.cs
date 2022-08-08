using SQLite;
namespace ImageLibrary.Model;

[Table("imagefiles")]
public class ImageFileInfo
{

    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    [Indexed]
    public string Name { get; set; }

    [Indexed]
    public string Description { get; set; } = string.Empty;//Optional notes. (will be used in searches)

    public string Image { get; set; } = string.Empty;// Full file path or file url.

    public string Location { get; set; } = string.Empty;//Original file path if svg file. Otherwise same value as Image property.
    public string FileName { get; set; }//File name only. No path
}
