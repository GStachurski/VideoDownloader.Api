using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDownloader.Api.Models;

namespace VideoDownloader.Api.Interfaces
{
    public interface IVideoService
    {
        Task<IEnumerable<YoutubeVideoResult>> GetYoutubeVideos(IEnumerable<Download> downloads);

        Task<IEnumerable<VideoDownloadResult>> GetVideoDownloads(IEnumerable<YoutubeVideoResult> youtubeVideos);

        IEnumerable<VideoDownloadResult> MapPartialDownloadsToDownloadResults(IEnumerable<PartialDownload> partialDownloads);
    }
}