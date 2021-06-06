using LightInject.Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace VideoDownloader.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureAppConfiguration((context, builder) =>
                   {
                       builder.AddJsonFile("usersettings.json", optional: true);
                   })
                   .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                                                                       .ReadFrom
                                                                       .Configuration(hostingContext.Configuration))
                   .UseLightInject()
                   .UseStartup<Startup>();
    }
}
