namespace AccentMSAddins.Services
{
    using AccentMSAddins.Services.Models;

    public interface ISlideUpdateManagerService
    {
        object CheckForUpdates(CheckUpdateDto data);

        bool VerifyFile(CheckUpdateDto data);
    }
}
