using System.Collections.Generic;
using VideoDownloader.Api.Models;

namespace VideoDownloader.Api.Interfaces
{
    public interface IDataParsingService
    {
        List<EditWindow> GetVideoEditWindows(Download download);

        bool IsValidDownloadUrl(string downloadUrl);

        string CleanTitle(string s);
    }
}
