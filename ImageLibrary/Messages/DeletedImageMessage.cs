

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ImageLibrary.Messages;

public class DeletedImageMessage : ValueChangedMessage<ImageFileInfo>
{
    public DeletedImageMessage(ImageFileInfo value) : base(value)
    {
    }
}
