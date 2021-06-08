using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using VideoDownloader.Api.Interfaces;
using VideoDownloader.Api.Models;
using VideoDownloader.Api.Options;

namespace VideoDownloader.Api.Services
{
    public class DataParsingService : IDataParsingService
    {
        private readonly ApiOptions _apiOptions;

        public DataParsingService(IOptions<ApiOptions> apiOptions)
        {
            _apiOptions = apiOptions.Value;
        }

        public List<EditWindow> GetVideoEditWindows(Download download)
        {
            var editWindows = new List<EditWindow>();

            if (string.IsNullOrEmpty(download.EditTimes))
            {
                return editWindows;
            }

            if (download.EditTimes.Contains(',', StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: check for a split limit here eventually (maybe 10 o 20?)
                var editTimeList = download.EditTimes.Split(',').ToList();
                editWindows.AddRange(from editTime in editTimeList select GetEditWindow(editTime));
            }
            else
            {
                editWindows.Add(GetEditWindow(download.EditTimes));
            }

            return editWindows;
        }

        public bool IsValidDownloadUrl(string downloadUrl)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(downloadUrl, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        private EditWindow GetEditWindow(string editWindowStr)
        {
            var editWindow = new EditWindow();
            if (editWindowStr.Contains('-', StringComparison.InvariantCultureIgnoreCase))
            {
                var editTimes = editWindowStr.Split('-');
                if (editTimes.Length != 2) return editWindow;

                _ = new TimeSpan();
                if (TimeSpan.TryParse(editTimes[0], out TimeSpan startTs))
                {
                    editWindow.StartTime = startTs;
                }

                _ = new TimeSpan();
                if (TimeSpan.TryParse(editTimes[1], out TimeSpan endTs))
                {
                    editWindow.EndTime = endTs;
                }
            }

            return editWindow;
        }
    }
}
