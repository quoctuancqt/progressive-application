namespace AccentMSAddins.Services
{
    using AccentMSAddins.Services.Models;

    public interface ICategoryService
    {
        string GetFileCatPath(CheckUpdateDto data, bool useWeb);

        LibraryCategoryInfo GetLibraryCategoryInfo(CheckUpdateDto data, int catId, bool useWeb, bool isIncludeFile = false);
    }
}
