namespace AccentMSAddins.Services.Models
{
    using AccentMSAddins.Services.Enum;
    using System;
    using System.Xml.Serialization;

    public class ContentItem
    {
        public ContentType ContentType { get; set; }
        public string FileId { get; set; }
        public int SlideId { get; set; }
        public int SlideNumber { get; set; }
        public int PageCount { get; set; }
        public string LastModDate { get; set; }
        public ContentType ParentContentType { get; set; }
        public string ParentContentId { get; set; }
        public int ParentItemIndex { get; set; }
        public string ParentLastModDate { get; set; }
        [XmlIgnore]
        public int ContentId
        {
            get { return ContentType == ContentType.Slide ? SlideId : int.Parse(FileId); }
        }
        [XmlIgnore]
        public int FileSize { get; set; }
        [XmlIgnore]
        public string SlideTitle { get; set; }
        [XmlIgnore]
        public string Filename { get; set; }
        [XmlIgnore]
        public string FileExtension { get; set; }
        [XmlIgnore]
        public int FirstSlideNum
        {
            get { return SlideNumber > 0 ? SlideNumber : 1; }
        }
        [XmlIgnore]
        public int LastSlideNum
        {
            get
            {
                int lastSlideNum = SlideNumber > 0 ? SlideNumber : PageCount;
                if (lastSlideNum < 1)
                {
                    lastSlideNum = FirstSlideNum; // special case for content groups with type ContentGroupType.Basic
                }
                return lastSlideNum;
            }
        }

        public ContentItem()
        {
            ContentType = ContentType.Undefined;
        }

        public ContentItem(ContentType contentType, int fileId, string filename, string fileExtension,
                            DateTime? lastModDate, int pageCount, int fileSize, int slideNum, int slideId, string slideTitle)
            : this()
        {
            ContentType = contentType;
            FileId = fileId.ToString();
            Filename = filename;
            FileExtension = fileExtension;
            LastModDate = lastModDate.Value.ToString();
            PageCount = pageCount;
            SlideId = slideId;
            SlideNumber = slideNum;
            SlideTitle = slideTitle;
            if (contentType == ContentType.File)
                FileSize = fileSize;

            // Default to something, these values will get overridden if parent is group or favorite
            ParentContentType = contentType;
            ParentContentId = fileId.ToString();
            ParentItemIndex = slideNum;
            ParentLastModDate = lastModDate.Value.ToString();
        }

        public override string ToString()
        {
            string toString = Filename;
            if (ContentType == ContentType.Slide)
            {
                toString = string.Format("Slide {0} of {1}", SlideNumber, Filename);
            }
            return toString;
        }
    }
}
