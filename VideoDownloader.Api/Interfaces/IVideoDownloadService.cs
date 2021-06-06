using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VideoDownloader.Api.Models;

namespace VideoDownloader.Api.Interfaces
{
    public interface IVideoDownloadService
    {
        Task<HttpStatusCode> DownloadAndEdit(List<Download> downloads);

        Task<HttpStatusCode> Status();
    }
}
