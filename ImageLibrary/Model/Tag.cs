using SQLite;
namespace ImageLibrary.Model;

[Table("tags")]
public class Tag
{

    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    [Indexed]
    public string Name { get; set; }

    public int InstancesCount { get; set; } = 0;

}
