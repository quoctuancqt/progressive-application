namespace AccentMSAddins.Services
{
    using AccentMSAddins.Services.Models;
    using DocumentFormat.OpenXml.Packaging;
    using System;

    public interface ITagManagerService
    {
        SlideTagExCollection GetSlideTags(PresentationPart presentationPart);

        FileTag GetFileTag(PresentationPart presentationPart);

        bool SetTags(CheckUpdateDto data, bool overwriteExistingTags);
    }
}
