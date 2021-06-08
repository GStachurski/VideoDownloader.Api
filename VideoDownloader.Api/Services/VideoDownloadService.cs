using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VideoDownloader.Api.Interfaces;
using VideoDownloader.Api.Models;
using VideoDownloader.Api.Options;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace VideoDownloader.Api.Services
{
    public class VideoDownloadService : IVideoDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiOptions _apiOptions;
        private IDataParsingService _parsingService;

        public VideoDownloadService(HttpClient httpClient, IOptions<ApiOptions> options, IDataParsingService parsingService)
        {
            _httpClient = httpClient;
            _apiOptions = options.Value;
            _parsingService = parsingService;
        }

        public async Task<IEnumerable<VideoDownloadResult>> GetDownloads(IEnumerable<Video> videos)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Video>> GetVideos(List<Download> downloads)
        {
            var youtubeClient = new YoutubeClient();
            var videoDownloadList = new List<Video>();

            foreach (var download in downloads)
            {
                if (_parsingService.IsValidDownloadUrl(download.Url))
                {
                    var video = await youtubeClient.Videos.GetAsync(download.Url);
                    videoDownloadList.Add(video);
                }
            };

            return videoDownloadList;
        }

        public Task<IEnumerable<Video>> GetVideosFromDownloads(List<Download> downloads)
        {
            throw new NotImplementedException();
        }

        public Task<HttpStatusCode> Status()
        {
            throw new NotImplementedException();
        }
    }
}
