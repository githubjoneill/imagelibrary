using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Model;

public class TagImageGroup : List<ImageFileInfo>
{
    public string Name { get; private set; }

	public TagImageGroup(string name, List<ImageFileInfo> imageFiles) : base(imageFiles)
	{
		Name = name;
	}

}
