using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

            // no edit windows (entire clip is used)
            if (string.IsNullOrEmpty(download.EditTimes))
            {
                return editWindows;
            }

            var editTimeList = download.EditTimes.Split(',').ToList();
            editTimeList = editTimeList.Skip(0).Take(_apiOptions.EditWindowLimit).ToList();
            editWindows.AddRange(from editTime in editTimeList select GetEditWindow(editTime));

            return editWindows;
        }

        public bool IsValidDownloadUrl(string downloadUrl)
        {
            bool result = _apiOptions.CheckForHttps
                ? Uri.TryCreate(downloadUrl, UriKind.Absolute, out Uri uriResult) 
                    && uriResult.Scheme == Uri.UriSchemeHttps
                : Uri.TryCreate(downloadUrl, UriKind.Absolute, out uriResult) 
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        public string CleanTitle(string s)
        {
            StringBuilder sb = new(s);
            sb.Replace("?", "");
            sb.Replace("'", "");
            sb.Replace(".", "");
            return sb.ToString();
        }

        private EditWindow GetEditWindow(string editWindowStr)
        {
            var editWindow = new EditWindow();
            if (editWindowStr.Contains('-', StringComparison.InvariantCultureIgnoreCase))
            {
                var edits = editWindowStr.Split('-');
                if (edits.Length != 2) return editWindow;

                Log.Information($"splitting times from window string {editWindowStr}");
                if (TryParseEditWindow(edits[0], out TimeSpan startTs))
                {
                    editWindow.StartTime = startTs;
                }
                if (TryParseEditWindow(edits[1], out TimeSpan endTs))
                {
                    editWindow.EndTime = endTs;
                }
            }

            return editWindow;
        }

        private bool TryParseEditWindow(string edit, out TimeSpan ts)
        {
            return TimeSpan.TryParseExact(edit, "m\\:ss", new CultureInfo("en-US"), TimeSpanStyles.None, out ts);
        }
    }
}
