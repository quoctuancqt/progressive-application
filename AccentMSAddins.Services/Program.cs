using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net;

namespace AccentMSAddins.Services
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseIISIntegration()
                .UseKestrel(opt => {
                    opt.AddServerHeader = false;
                    opt.Listen(IPAddress.Loopback, 9999, listenOpt=> {
                        listenOpt.UseHttps("accent-addins-cert.pfx", "123456");
                    });
                })
                .UseUrls("https://localhost:9999")
                .Build();
    }
}
