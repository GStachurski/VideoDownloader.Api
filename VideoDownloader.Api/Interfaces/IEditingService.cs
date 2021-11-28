using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDownloader.Api.Models;

namespace VideoDownloader.Api.Interfaces
{
    public interface IEditingService
    {
        Task<IEnumerable<VideoEditResult>> GetVideoEditResults(IEnumerable<VideoDownloadResult> videoDownloadResults);

        Task<IEnumerable<VideoEditResult>> NormalizeFrameratesFromEdits(IEnumerable<VideoEditResult> edits);

        Task<VideoEditResult> CreateVideoFromVideoClips(IEnumerable<VideoEditResult> videoEditResults, string fileName);
    }
}