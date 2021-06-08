using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VideoDownloader.Api.Models;
using YoutubeExplode.Videos;

namespace VideoDownloader.Api.Interfaces
{
    public interface IVideoDownloadService
    {
        Task<IEnumerable<Video>> GetVideos(List<Download> downloads);

        Task<IEnumerable<VideoDownloadResult>> GetDownloads(IEnumerable<Video> videos);

        Task<HttpStatusCode> Status();
    }
}
