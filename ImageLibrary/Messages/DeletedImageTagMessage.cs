using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ImageLibrary.Messages;

public class DeletedImageTagMessage : ValueChangedMessage<ImageTag>
{
    public DeletedImageTagMessage(ImageTag value) : base(value)
    {
    }
}
