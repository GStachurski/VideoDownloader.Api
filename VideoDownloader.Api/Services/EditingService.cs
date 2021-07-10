using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDownloader.Api.Interfaces;
using VideoDownloader.Api.Models;
using VideoDownloader.Api.Options;
using Xabe.FFmpeg;

namespace VideoDownloader.Api.Services
{
    public class EditingService : IEditingService
    {
        private readonly string _downloadPath;
        private readonly ApiOptions _apiOptions;
        public EditingService(IOptions<ApiOptions> options)
        {
            _apiOptions = options.Value;
            _downloadPath = _apiOptions.VideoSettings.DownloadPath;
        }

        public async Task<List<VideoEditResult>> GetVideoEditResults(IEnumerable<VideoDownloadResult> videoDownloadResults)
        {
            var results = new List<VideoEditResult>();
            int fileCount = 0;

            foreach (var videoResult in videoDownloadResults)
            {
                foreach (var edit in videoResult.EditWindows)
                {
                    // pad the end by a second just to make sure we get full clip
                    var fullTitle = $"{videoResult.Video.Title}{fileCount}.{videoResult.HqVideoStream.Container}";
                    var fullPath = $"{_downloadPath}{fullTitle}";
                    var videoOutputPath = fullPath;

                    var timeSpanDiff = (edit.EndTime - edit.StartTime) + TimeSpan.FromSeconds(1);
                    IConversion conversion = await FFmpeg.Conversions.FromSnippet.Split(videoResult.Location, videoOutputPath, edit.StartTime, timeSpanDiff);
                    IConversionResult result = await conversion.Start();

                    var chopResult = new VideoEditResult { Location = videoOutputPath, Order = videoResult.Order };
                    fileCount++;
                }
            }

            return results;
        }
    }
}
