﻿using Microsoft.Extensions.Options;
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
using YoutubeExplode.Videos;
using ByteSizeLib;
using System.Threading;
using System.Linq;

namespace VideoDownloader.Api.Services
{
    public class VideoDownloadService : IVideoDownloadService
    {
        private readonly string _downloadPath;
        private readonly ApiOptions _apiOptions;
        private readonly YoutubeClient _youtubeClient;
        private readonly IDataParsingService _parsingService;
        public VideoDownloadService(IOptions<ApiOptions> options, IDataParsingService parsingService)
        {
            _apiOptions = options.Value;
            _parsingService = parsingService;

            _youtubeClient = new YoutubeClient();            
            _downloadPath = _apiOptions.VideoSettings.DownloadPath;
        }
        public async Task<IEnumerable<VideoDownloadResult>> DownloadVideos(IEnumerable<Video> videos)
        {
            var videoDownloadResults = new List<VideoDownloadResult>();
            foreach (var video in videos)
            {
                Log.Information($"getting stream manifest for video id '{video.Id}' with duration of ({video.Duration})");
                StreamManifest manifests = new StreamManifest(new List<IStreamInfo>());
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

                        try
                        {
                            // path contains ending slash
                            Log.Information(@$"downloading {fullVideoTitle} {hqAud.Bitrate} {hqVid.VideoQuality} {hqVid.VideoResolution}");
                            await RetryPolicyHandler.RetryPolicy().ExecuteAsync(async () =>
                            {
                                var cts = new CancellationTokenSource(); cts.CancelAfter(TimeSpan.FromMinutes(1));
                                await _youtubeClient.Videos.DownloadAsync(
                                     new IStreamInfo[] { hqAud, hqVid },
                                         new ConversionRequestBuilder(fullPath)                                             
                                             .SetFFmpegPath(_apiOptions.VideoSettings.FFmpegPath)
                                             .Build(), 
                                         cancellationToken : cts.Token);
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"error while attempting to download video {hqVid.Url}");
                            throw ex;
                        }

                        videoDownloadResults.Add(new VideoDownloadResult
                        {
                            Title = fullVideoTitle,
                            IsSuccessful = true,
                            Location = _apiOptions.VideoSettings.DownloadPath,
                            Size = ByteSize.FromMegaBytes(hqVid.Size.MegaBytes).ToString()
                        });                    
                }
            }

            return videoDownloadResults;
        }        

        public async Task<IEnumerable<Video>> GetVideos(List<Download> downloads)
        {
            var videoDownloadList = new List<Video>();
            foreach (var download in downloads)
            {
                if (_parsingService.IsValidDownloadUrl(download.Url))
                {
                    var video = await _youtubeClient.Videos.GetAsync(download.Url);
                    videoDownloadList.Add(video);
                }
            };
            return videoDownloadList;
        }
    }
}
