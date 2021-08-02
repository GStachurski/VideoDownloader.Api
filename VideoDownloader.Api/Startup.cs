using LightInject;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Linq;
using VideoDownloader.Api.Interfaces;
using VideoDownloader.Api.Options;
using VideoDownloader.Api.Services;
using Xabe.FFmpeg;

namespace VideoDownloader.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Need LightInject for some advanced DI that is not implemented out of the box
            var containerOptions = new ContainerOptions
            {
                EnablePropertyInjection = false,
                DefaultServiceSelector = serviceNames
                    => serviceNames.SingleOrDefault(string.IsNullOrWhiteSpace) ?? serviceNames.Last(),
                EnableVariance = false
            };

            var container = new ServiceContainer(containerOptions);
            services.AddControllersWithViews();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // add the api options
            services.AddOptions();
            services.Configure<ApiOptions>(Configuration.GetSection(nameof(ApiOptions)));

            // data and video downloader services
            services.AddSingleton<IDataParsingService, DataParsingService>();
            services.AddSingleton<IVideoService, VideoService>();
            services.AddSingleton<IEditingService, EditingService>();

            // ffmpeg
            ConfigureFFmpeg();

            services.AddMvc()
                .AddControllersAsServices()
                .AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Video Downloader API",
                    Version = "v1",
                    Description = "This API provides an interface for downloading videos, editing them, and outputting them as one file."
                });

            });
            services.AddControllers();
        }

        public void ConfigureFFmpeg()
        {
            var ffmpegDir = Configuration
                .GetSection(nameof(ApiOptions))
                .Get<ApiOptions>()
                .VideoSettings
                .FFmpegDirectory;
            Log.Information($"loading ffmpegDirectory from {ffmpegDir}");
            FFmpeg.SetExecutablesPath(ffmpegDir);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));
            app.UseCors("DefaultPolicy");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "VideoDownloader API");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
