using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private DataParsingService _parsingService;

        public VideoDownloadService(HttpClient httpClient, ApiOptions options, DataParsingService parsingService)
        {
            _httpClient = httpClient;
            _apiOptions = options;
            _parsingService = parsingService;
        }

        public async Task<IEnumerable<VideoDownloadResult>> GetDownloadResults(IEnumerable<Video> videos)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Video>> GetVideoManifests(List<Download> downloads)
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
