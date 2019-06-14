namespace AccentMSAddins.Services.Common
{
    using AccentMSAddins.Services.Enum;

    public static class CategoryTypeExtension
    {
        public static bool IsUserCategory(this CategoryType categoryType)
        {
            return categoryType == CategoryType.Uploads || categoryType == CategoryType.Downloads || categoryType == CategoryType.Proposals;
        }
    }
}
