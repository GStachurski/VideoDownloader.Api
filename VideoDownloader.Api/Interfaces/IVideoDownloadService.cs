using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDownloader.Api.Models;
using YoutubeExplode.Videos;

namespace VideoDownloader.Api.Interfaces
{
    public interface IVideoDownloadService
    {
        Task<IEnumerable<Video>> GetVideos(List<Download> downloads);

        Task<IEnumerable<VideoDownloadResult>> DownloadVideos(IEnumerable<Video> videos);
    }
}