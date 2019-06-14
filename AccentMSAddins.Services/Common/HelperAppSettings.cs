namespace AccentMSAddins.Services.Common
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;

    public class HelperAppSettings
    {
        private static IConfiguration _configuration;

        private static IHostingEnvironment _hostingEnvironment;

        public HelperAppSettings(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;

            _hostingEnvironment = environment;
        }

        public static IHostingEnvironment HostingEnvironment
        {
            get
            {
                return _hostingEnvironment;
            }
        }
    }
}
