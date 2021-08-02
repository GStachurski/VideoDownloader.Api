using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoDownloader.Api.Interfaces;
using VideoDownloader.Api.Models;
using VideoDownloader.Api.Options;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Converter;
using System.Threading;
using System.Linq;
using System.IO;
using YoutubeExplode.Videos;

namespace VideoDownloader.Api.Services
{
    public class VideoService : IVideoService
    {
        private readonly string _downloadPath;
        private readonly ApiOptions _apiOptions;
        private readonly YoutubeClient _youtubeClient;
        private readonly IDataParsingService _parsingService;
        public VideoService(IOptions<ApiOptions> options, IDataParsingService parsingService)
        {
            _apiOptions = options.Value;
            _parsingService = parsingService;

            _youtubeClient = new YoutubeClient();            
            _downloadPath = _apiOptions.VideoSettings.DownloadPath;
        }
        public async Task<IEnumerable<YoutubeVideoResult>> GetYoutubeVideos(IEnumerable<Download> downloads)
        {
            var videoDownloadList = new List<YoutubeVideoResult>();

            foreach (var download in downloads)
            {
                if (_parsingService.IsValidDownloadUrl(download.Url))
                {
                    Video video = null;
                    await RetryPolicyHandler.RetryPolicy().ExecuteAsync(async () =>
                    {
                        Log.Information($"getting video metadata for download '{download.Name}'");
                        video = await _youtubeClient.Videos.GetAsync(download.Url);
                    });

                    var manifestResult = new YoutubeVideoResult
                    {
                        EditWindows = _parsingService.GetVideoEditWindows(download),
                        Order = download.EditOrder.Value,
                        Video = video
                    };

                    videoDownloadList.Add(manifestResult);
                }
            };

            return videoDownloadList;
        }

        public async Task<IEnumerable<VideoDownloadResult>> GetVideoDownloads(IEnumerable<YoutubeVideoResult> youtubeVideos)
        {
            var videoDownloadResults = new List<VideoDownloadResult>();

            foreach (var manifest in youtubeVideos)
            {
                var video = manifest.Video;
                Log.Information($"getting stream manifest for video id '{video.Id}' with duration of ({video.Duration})");
                StreamManifest manifests = new (new List<IStreamInfo>());
                await RetryPolicyHandler.RetryPolicy().ExecuteAsync(async () =>
                {
                    var cts = new CancellationTokenSource(); cts.CancelAfter(TimeSpan.FromSeconds(15));
                    manifests = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id, cts.Token);
                });

                if (manifests.Streams.Any())
                {
                    // get best audio and video streams if we're downloading them seperately
                    var hqAud = manifests.GetAudioOnlyStreams().GetWithHighestBitrate();
                    var hqVid = manifests.GetVideoOnlyStreams().GetWithHighestVideoQuality();

                    var fullVideoTitle = $"{video.Title}.{hqVid.Container.Name}";
                    var fullPath = $"{_downloadPath}{fullVideoTitle}";

                    // see if it already exists
                    if (!File.Exists(fullPath))
                    {
                        try
                        {
                            // path contains ending slash
                            Log.Information(@$"downloading {fullVideoTitle} {hqAud.Bitrate} {hqVid.VideoQuality} {hqVid.VideoResolution}");
                            await RetryPolicyHandler.RetryPolicy().ExecuteAsync(async () =>
                            {
                                var cts = new CancellationTokenSource(); 
                                cts.CancelAfter(TimeSpan.FromMinutes(_apiOptions.VideoSettings.DownloadTimeout));
                                await _youtubeClient.Videos.DownloadAsync(
                                     new IStreamInfo[] { hqAud, hqVid },
                                         new ConversionRequestBuilder(fullPath)
                                             .SetFFmpegPath(_apiOptions.VideoSettings.FFmpegPath)
                                             .Build(),
                                         cancellationToken: cts.Token);
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"error while attempting to download video {hqVid.Url}");
                            throw ex;
                        }
                    }

                    videoDownloadResults.Add(new VideoDownloadResult
                    {
                        EditWindows = manifest.EditWindows,
                        HqVideoStream = hqVid,
                        IsSuccessful = true,
                        Location = fullPath,
                        Order = manifest.Order,
                        Title = fullVideoTitle,
                        Video = video
                    });
                }
            }

            return videoDownloadResults;
        }
    }
}
