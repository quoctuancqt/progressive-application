namespace AccentMSAddins.Services.Enum
{
    using System;
    using System.Xml.Serialization;

    public enum ContentType
    {
        [XmlEnum("0")] Undefined = 0,
        [XmlEnum("1")] File = 1,
        [XmlEnum("2")] Category = 2,
        [XmlEnum("3")] Master = 3,
        [XmlEnum("4")] GlobalFavorite = 4,
        [XmlEnum("4")] ContentGroup = GlobalFavorite,
        //[XmlEnum("5")]SharedFavoriteCategory = 5,
        //[XmlEnum("6")]SharedFavorite = 6,
        [XmlEnum("7")] EventGroup = 7,
        [XmlEnum("8")] SupportFile = 8,
        [XmlEnum("9")] WorkItem = 9,
        [XmlEnum("10")] Xml = 10,
        [XmlEnum("11")] MyFavorite = 11,
        [XmlEnum("11")] VirtualPresentation = MyFavorite,
        [XmlEnum("12")] Slide = 12,
        [XmlEnum("13")] MyFavoriteCategory = 13,
        [XmlEnum("14")] Event = 14,
        [XmlEnum("15")] CrmAccount = 15,
        [XmlEnum("16")] CrmOpportunity = 16,
        [XmlEnum("17")] CrmTask = 17,
        [XmlEnum("18")] CrmIssue = 18,
        [XmlEnum("19")] CrmBuyer = 19,
        [XmlEnum("20")] Article = 20,
        [XmlEnum("21")] ArticleSection = 21,
        [XmlEnum("22")] CollaborationTopic = 22,
        [XmlEnum("23")] CollaborationMessage = 23,
        [XmlEnum("24")] Comment = 24,
        [XmlEnum("25")] Library = 25,
        [XmlEnum("26")] CollaborationCategory = 26,
        [XmlEnum("27")] Homepage = 27,
        [XmlEnum("28")] Wizard = 28,
        [XmlEnum("-1")] Default = -1,
    }

    public interface IContentInfo
    {
        int ContentId { get; }
        ContentType ContentType { get; }
        string Name { get; }
    }

    public class ContentInfo : IContentInfo
    {
        public int ContentId { get; set; }
        public ContentType ContentType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
    }
}
