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
using System;
using System.IO;
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
            services.Configure<ApiOptions>(Configuration.GetSection(nameof(ApiOptions)));            
            services.AddSingleton<IDataParsingService, DataParsingService>();

            // ffmpeg
            ConfigureFFmpeg();

            // video download service
            services.AddHttpClient<IVideoDownloadService, VideoDownloadService>();
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
            var ffmpegPath = Configuration.GetValue<string>("ApiOptions:VideoFFmpegPath");
            FFmpeg.SetExecutablesPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg"));
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
