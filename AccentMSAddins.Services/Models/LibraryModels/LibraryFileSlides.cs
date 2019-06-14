namespace AccentMSAddins.Services.Models
{
    public class LibraryFileSlides
    {
        public LibraryFileSlides()
        {
            Slides = new SlideInfoCollection();
        }

        public LibraryFileSlides(LibraryFileResultInfo fileInfo, SlideInfoCollection slides)
        {
            FileInfo = fileInfo;
            Slides = slides;
        }

        public LibraryFileResultInfo FileInfo { get; set; }
        public SlideInfoCollection Slides { get; set; }
    }
}
