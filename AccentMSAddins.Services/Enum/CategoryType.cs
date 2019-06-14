namespace AccentMSAddins.Services.Enum
{
    using System.Xml.Serialization;

    public enum CategoryType
    {
        [XmlEnum("0")] Generic = 0,
        [XmlEnum("1")] GlobalFavorites = 1,
        [XmlEnum("2")] Uploads = 2,
        [XmlEnum("3")] Downloads = 3,
        [XmlEnum("4")] Proposals = 4,
        [XmlEnum("5")] EventGroups = 5,
        [XmlEnum("6")] MyFavorites = 6,
        [XmlEnum("7")] Workflow = 7,
        [XmlEnum("8")] Articles = 8,
        [XmlEnum("9")] DataSource = 9,
        [XmlEnum("-1")] Undefined = -1,
    }
}
