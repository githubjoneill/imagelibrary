using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ImageLibrary.Messages;


public class ChangedImageTagMessage : ValueChangedMessage<ImageTag>
{
    public ChangedImageTagMessage(ImageTag value) : base(value)
    {
    }
}
