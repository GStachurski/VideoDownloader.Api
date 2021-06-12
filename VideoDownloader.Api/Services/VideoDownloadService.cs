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
using YoutubeExplode.Videos;

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
                var manifests = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);                
                if (manifests != null)
                {
                    if (_apiOptions.SeperateAudioAndVideoStreams)
                    {
                        // get best audio and video streams if we're downloading them seperately
                        var hqAudio = manifests.GetAudioOnlyStreams().GetWithHighestBitrate();
                        var hqVideo = manifests.GetVideoOnlyStreams().GetWithHighestVideoQuality();
                        Log.Information($"getting stream for audio bitrate '{hqAudio.Bitrate}' and video quality '{hqVideo.VideoQuality}' and resolution '{hqVideo.VideoResolution}')");

                        try
                        {
                            // path contains ending slash
                            var fullPath = $"{_downloadPath}{video.Title}.{hqVideo.Container.Name}";
                            await _youtubeClient.Videos.DownloadAsync(
                                new IStreamInfo[] { hqAudio, hqVideo },
                                    new ConversionRequestBuilder(fullPath)
                                        .SetPreset(ConversionPreset.Fast)
                                        .SetFFmpegPath(_apiOptions.VideoSettings.FFmpegPath)
                                        .Build());
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"error while attempting to download video {hqVideo.Url}");
                            throw ex;
                        }

                        videoDownloadResults.Add(new VideoDownloadResult { IsSuccessful = true, Location = _apiOptions.VideoSettings.DownloadPath, Size = hqVideo.Size.MegaBytes });
                    }
                    else
                    {
                        // get best audio and video streams already muxed together
                        var hqVid = manifests.GetMuxedStreams().TryGetWithHighestVideoQuality();
                        if (hqVid != null)
                        {
                            Log.Information($"getting stream for bitrate '{hqVid.Bitrate}' and quality '{hqVid.VideoQuality}' and resolution '{hqVid.VideoResolution}')");
                            try
                            {
                                await _youtubeClient.Videos.Streams.DownloadAsync(hqVid, $"{_downloadPath}\\{video.Title}.{hqVid.Container}");
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, $"error while attempting to download video {hqVid.Url}");
                                throw ex;
                            }

                            videoDownloadResults.Add(new VideoDownloadResult { IsSuccessful = true, Location = _apiOptions.VideoSettings.DownloadPath, Size = hqVid.Size.MegaBytes });
                        }
                    }
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
