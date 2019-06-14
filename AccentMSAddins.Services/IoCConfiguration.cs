namespace AccentMSAddins.Services
{
    using Microsoft.Extensions.DependencyInjection;

    public class IoCConfiguration
    {
        public static void RegisterIoC(IServiceCollection services)
        {
            services.AddScoped<IFilesService, FilesService>();
            services.AddScoped<ISlideUpdateManagerService, SlideUpdateManagerService>();
            services.AddScoped<ITagManagerService, TagManagerService>();
            services.AddScoped<ICategoryService, CategoryService>();
        }
    }
}
