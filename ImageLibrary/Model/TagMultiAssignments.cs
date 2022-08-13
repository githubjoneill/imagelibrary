using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLibrary.Model
{
    public partial class TagMultiAssignments : ObservableObject
    {
        public List<Tag> tagsAssigned = new();

        public List<Tag> tagsAvailable =new();

        public List<ImageFileInfo> images = new();
    }
}
