using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDownloader.Api.Models;

namespace VideoDownloader.Api.Interfaces
{
    public interface IEditingService
    {
        Task<List<VideoEditResult>> GetVideoEditResults(IEnumerable<VideoDownloadResult> videoDownloadResults);
    }
}