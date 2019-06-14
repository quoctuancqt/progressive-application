namespace AccentMSAddins.Services.Models
{
    public class SlideUpdatedResultDto
    {
        public SlideTagEx Tag { get; set; }
        public int PptSlideIndex { get; set; }
        public string Status { get; set; }
        public string LastModDate { get; set; }
        public string Source { get; set; }
        public string Id { get; set; }
        public bool IsUpdated { get; set; }
    }
}
