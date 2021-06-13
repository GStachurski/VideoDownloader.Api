using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode;
using VideoDownloader.Api.Models;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using System;

namespace VideoDownloader.Api.Tests
{
    [TestClass]
    public class VideoCompilingTests
    {
        [TestMethod]
        public void FFmpegExecutablesAreAccessible()
        {
            // arrange
            var ffMpegPath = "C:\\FFMPEG\\ffmpeg.exe";
            var fullPath = "C:\\Videos\\test.mp4";
            var youtubeClient = new YoutubeClient();
            var download = new Download() 
            { 
                Id = 1, 
                Name = "Oval Barn", 
                Url = "https://www.youtube.com/watch?v=HqBhCibidNM"
            };

            // act
            Task.Run(async () =>
            {
                var video = await youtubeClient.Videos.GetAsync(download.Url);
                var manifests = await youtubeClient.Videos.Streams.GetManifestAsync(video.Id);
                var hqAudio = manifests.GetAudioOnlyStreams().GetWithHighestBitrate();
                var hqVideo = manifests.GetVideoOnlyStreams().GetWithHighestVideoQuality();

                await youtubeClient.Videos.DownloadAsync(
                    new IStreamInfo[] { hqAudio, hqVideo },
                        new ConversionRequestBuilder(fullPath)
                            .SetPreset(YoutubeExplode.Converter.ConversionPreset.Fast)
                            .SetFFmpegPath(ffMpegPath)
                            .Build());
            }).GetAwaiter().GetResult();

            // assert
            Assert.IsTrue(File.Exists(fullPath));
        }
    

        [TestMethod]
        public void FFmpegCanChopUpVideos()
        {
            // arrange
            var finalDuration = new TimeSpan();
            FFmpeg.SetExecutablesPath(@"C:\FFMPEG\");
            var videoPath = @"C:\Videos\bobross_video_sample.mp4";
            var videoOutputPath = @"C:\Videos\bobross_video_sample_chopped.mp4";            
            var editWindow = new EditWindow {
                StartTime = new TimeSpan(0, 11, 35),
                EndTime = new TimeSpan(0, 17, 40)
            };
            var timeSpanDiff = editWindow.EndTime - editWindow.StartTime;

            // act
            Task.Run(async () =>
            {
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
                IConversion conversion = await FFmpeg.Conversions.FromSnippet.Split(videoPath, videoOutputPath, editWindow.StartTime, timeSpanDiff);
                IConversionResult result = await conversion.Start();
                finalDuration = result.Duration;
            }).GetAwaiter().GetResult();

            // assert
            Assert.IsTrue(File.Exists(videoOutputPath));
        }
    }
}
