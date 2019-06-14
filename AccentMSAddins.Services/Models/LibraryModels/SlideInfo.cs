namespace AccentMSAddins.Services.Models
{
    using AccentMSAddins.Services.Enum;
    using System;
    using System.Collections;
    using System.IO;
    using System.Linq;

    public class SlideInfo : IContentInfo
    {
        public const int MAX_FILE_FIELD_COUNT = 12;

        public int FileId { get; set; }
        public string FileName { get; set; }
        public int SlideId { get; set; }
        public int SlidePptId { get; set; }
        public int SlideNumber { get; set; }
        public short SlideUpdate { get; set; }
        public bool Editable { get; set; }
        public DateTime? LastModDate { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public ExtraFields ExtraFields { get; set; }
        public CategoryType CatType { get; set; }
        public string Name
        {
            get { return string.IsNullOrEmpty(Title) ? Path.GetFileNameWithoutExtension(FileName) : Title; }
        }

        public int ContentId { get { return SlideId; } }
        public ContentType ContentType { get { return ContentType.Slide; } }

        public string SlideType
        {
            get
            {
                string ext = Path.GetExtension(FileName);
                if (!string.IsNullOrEmpty(ext) && ext.ToLower().StartsWith("ppt"))
                    return "Slide";

                return "Page";
            }
        }

        public long FileHash { get; set; }
        public string[] Keywords { get; set; }
    }

    public class SlideInfoCollection : CollectionTypeDescriptor<SlideInfo, SlideInfoDescriptor>
    {
        public bool ContainsPptId(int pptId)
        {
            return this.Any(slide => slide.SlidePptId == pptId);
        }
    }

    public class SlideInfoDescriptor : CollectionDescriptor
    {
        public SlideInfoDescriptor()
        {

        }

        public SlideInfoDescriptor(IList collection, int index)
            : base(collection, index)
        {
        }

        public override string DisplayName
        {
            get
            {
                SlideInfo info = _collection[_index] as SlideInfo;
                return "Slide #" + info.SlideNumber;
            }
        }
    }
}
