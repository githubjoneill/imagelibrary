
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ImageLibrary.Messages;


public class ChangedImageMessage : ValueChangedMessage<ImageFileInfo>
{
    public ChangedImageMessage(ImageFileInfo value) : base(value)
    {
    }
}
