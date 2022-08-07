using SQLite;


namespace ImageLibrary.Model;

[Table("imagetags")]
public class ImageTag
{

    [PrimaryKey, AutoIncrement, Column("_id")]
    public int ID { get; set; }

    [NotNull]
    public int ImageId { get; set; }

    [NotNull]
    public int TagId { get; set; }

}
