using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VideoDownloader.Api.Models;
using YoutubeExplode.Videos;

namespace VideoDownloader.Api.Interfaces
{
    public interface IVideoDownloadService
    {
        Task<IEnumerable<Video>> GetVideosFromDownloads(List<Download> downloads);

        Task<IEnumerable<VideoDownloadResult>> GetDownloadResults(IEnumerable<Video> videos);

        Task<HttpStatusCode> Status();
    }
}
