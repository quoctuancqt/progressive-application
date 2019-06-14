namespace AccentMSAddins.Services
{
    using AccentMSAddins.Services.Models;
    using DocumentFormat.OpenXml.Packaging;
    using System.IO;

    public interface IFilesService
    {
        void InsertCustomXml(CheckUpdateDto data);

        LibraryFileInfoEx GetFileInfo(CheckUpdateDto data);

        LibraryFileSlides GetFileSlidesInfo(CheckUpdateDto data);

        string GetLatestFile(CheckUpdateDto data);

        string GetCustomXmlPart(PresentationPart presentationPart);

        Stream OpenFile(string webClientUrl, string shortLibraryName, string authHash, string fileId);
    }
}
