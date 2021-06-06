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

namespace VideoDownloader.Api.Services
{
    public class VideoDownloadService : IVideoDownloadService
    {
        private readonly HttpClient _httpClient;

        public VideoDownloadService(HttpClient httpClient, IOptions<ApiOptions> options)
        {

        }

        public Task<HttpStatusCode> DownloadAndEdit(List<Download> downloads)
        {
            throw new NotImplementedException();
        }

        public Task<HttpStatusCode> Status()
        {
            throw new NotImplementedException();
        }
    }
}
