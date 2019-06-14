namespace AccentMSAddins.Services.Models
{
    using AccentMSAddins.Services.Enum;
    using System.Collections.Generic;

    public class LibrarianContentManifest
    {
        public string DateCreatedUTC { get; set; }
        public string Server { get; set; }
        public string LibraryName { get; set; }
        public bool LiveDb { get; set; }
        public CategoryType CatType { get; set; }
        public ContentType ContentType { get; set; }
        public string ContentId { get; set; }
        public string Filename { get; set; }
        public string LastModDate { get; set; }
        public int PageCount { get; set; }
        public List<ContentItem> Items { get; set; }
    }
}
