using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly IDataParsingService _dataParsingService;
        public EditingService(IOptions<ApiOptions> options, IDataParsingService dataParsingService)
        {
            _dataParsingService = dataParsingService;
            _apiOptions = options.Value;
            _downloadPath = _apiOptions.VideoSettings.DownloadPath;
        }
        public async Task<IEnumerable<VideoEditResult>> GetVideoEditResults(IEnumerable<VideoDownloadResult> videoDownloadResults)
        {
            var results = new List<VideoEditResult>();
            int fileCount = 0;

            foreach (var videoResult in videoDownloadResults)
            {
                // if there are no edit windows, use the whole video
                var cleanTitle = _dataParsingService.CleanTitle(videoResult.Video.Title);
                if (!videoResult.EditWindows.Any())
                {
                    var fullTitle = $"{cleanTitle}.{videoResult.HqVideoStream.Container}";
                    var fullVideo = new VideoEditResult { Location = $"{_downloadPath}{fullTitle}", Order = videoResult.Order };
                    results.Add(fullVideo);
                    fileCount++;
                }
                // else, split up the video by edit windows
                else
                {                    
                    foreach (var edit in videoResult.EditWindows)
                    {
                        var fullTitle = $"{cleanTitle}{"_"}{fileCount}.{videoResult.HqVideoStream.Container}";
                        var fullPath = $"{_downloadPath}{fullTitle}";
                        var videoOutputPath = fullPath;

                        // it's possible the chop already exists
                        if (!File.Exists(videoOutputPath))
                        {
                            // pad the diff window by a second so we know the full clip is cropped
                            var timeSpanDiff = (edit.EndTime - edit.StartTime) + TimeSpan.FromSeconds(1);
                            IConversion conversion = await FFmpeg.Conversions.FromSnippet.Split(videoResult.Location, videoOutputPath, edit.StartTime, timeSpanDiff);
                            IConversionResult result = await conversion.Start();
                        }

                        var chop = new VideoEditResult { Location = videoOutputPath, Order = videoResult.Order };
                        results.Add(chop);
                        fileCount++;
                    }
                }
            }

            return results;
        }

        public async Task<VideoEditResult> CreateVideoFromVideoClips(IEnumerable<VideoEditResult> videoEditResults, string fileName)
        {
            // transform into a list and sort by video order
            var listEdits = videoEditResults.OrderBy(ed => ed.Order);
            var locList = listEdits.Select(le => le.Location).ToList();
            var editCounts = locList.Count();
            
            Log.Information($"attempting to concatenate {editCounts} videos");
            Log.Information($"building from the following video locations {Environment.NewLine}{string.Join($"{Environment.NewLine}", locList)}");
                        
            // setup the final file output location and name
            var finalFileName = $"{_downloadPath}{fileName}{"_"}{listEdits.Count()}{".mp4"}";
            var concantVideos = await FFmpeg                
                .Conversions
                .FromSnippet
                .Concatenate(finalFileName, (from edit in listEdits select edit.Location).ToArray());
            var result = await concantVideos
                .UseMultiThread(true)
                .Start();
            var info = await FFmpeg.GetMediaInfo(finalFileName);

            return new VideoEditResult 
            { 
                Location = finalFileName, 
                Size = info.Size,
                Length = info.Duration
            };
        }
    }
}