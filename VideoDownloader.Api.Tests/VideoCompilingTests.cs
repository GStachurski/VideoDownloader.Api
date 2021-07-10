using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode;
using VideoDownloader.Api.Models;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public void FFmpegSingleVideoChop()
        {
            // arrange
            var finalDuration = new TimeSpan();
            var videoPath = @"C:\Videos\bobross_video_sample1.mp4";
            var videoOutputPath = @"C:\Videos\bobross_video_sample1_chopped.mp4";
            FFmpeg.SetExecutablesPath(@"C:\FFMPEG\");

            var editWindow = new EditWindow {
                StartTime = new TimeSpan(0, 11, 35),
                EndTime = new TimeSpan(0, 17, 40)
            };

            // pad the end by a second just to make sure we get full clip
            var timeSpanDiff = (editWindow.EndTime - editWindow.StartTime) + TimeSpan.FromSeconds(1);

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

        [TestMethod]
        public void FFmpegDualVideoChop()
        {
            // arrange
            FFmpeg.SetExecutablesPath(@"C:\FFMPEG\");
            var finalDuration = new TimeSpan();
            var videoPath = @"C:\Videos\bobross_video_sample2.mp4";
            var videoOutputPath = @"C:\Videos\bobross_video_sample2_{0}_chopped.mp4";
            var editList = new List<EditWindow>()
            {
                new EditWindow { StartTime = new TimeSpan(0,4,43), EndTime = new TimeSpan(0,9,45) },
                new EditWindow { StartTime = new TimeSpan(0,14,26), EndTime = new TimeSpan(0,20,38) }
            };

            // act
            int fileCount = 0;
            foreach (var edit in editList)
            {
                // pad the end by a second just to make sure we get full clip
                var timeSpanDiff = (edit.EndTime - edit.StartTime) + TimeSpan.FromSeconds(1);
                var formatVideoOutputPath = string.Format(videoOutputPath, fileCount);

                Task.Run(async () =>
                {
                    IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
                    IConversion conversion = await FFmpeg.Conversions.FromSnippet.Split(videoPath, formatVideoOutputPath, edit.StartTime, timeSpanDiff);
                    IConversionResult result = await conversion.Start();
                    finalDuration = result.Duration;
                }).GetAwaiter().GetResult();
                fileCount++;
            }

            // assert
            Assert.AreEqual(fileCount, editList.Count);
        }

        [TestMethod]
        public void CanConcantenateVideos()
        {
            // arrange
            FFmpeg.SetExecutablesPath(@"C:\FFMPEG\");

            int fileCount = 0;
            var fileListAndIndex = new List<Tuple<int, string>>();

            var calculatedDuration = new TimeSpan();
            var finalDuration = new TimeSpan();

            var videoPath = @"C:\Videos\bobross_video_sample2.mp4";
            var videoOutputPath = @"C:\Videos\bobross_video_sample2_{0}_chopped.mp4";
            var videoFinalPath = @"C:\Videos\bobross_video_sample2_final.mp4";

            var editList = new List<EditWindow>()
            {
                new EditWindow { StartTime = new TimeSpan(0,4,43), EndTime = new TimeSpan(0,9,45) },
                new EditWindow { StartTime = new TimeSpan(0,14,26), EndTime = new TimeSpan(0,20,38) }
            };

            foreach (var window in editList)
            {
                calculatedDuration = calculatedDuration.Add(window.EndTime - window.StartTime);
            };

            // act
            // chop up two parts of the video into seperate files
            // : bobross_video_sample2_1_chopped.mp4 (05m02s)
            // : bobross_video_sample2_2_chopped.mp4 (06m12s)
            foreach (var edit in editList)
            {
                // pad the end by a second just to make sure we get full clip
                var timeSpanDiff = (edit.EndTime - edit.StartTime) + TimeSpan.FromSeconds(.1);
                var formatVideoOutputPath = string.Format(videoOutputPath, fileCount);

                Task.Run(async () =>
                {
                    IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
                    IConversion conversion = await FFmpeg.Conversions.FromSnippet.Split(videoPath, formatVideoOutputPath, edit.StartTime, timeSpanDiff);
                    IConversionResult result = await conversion.Start();                    
                }).GetAwaiter().GetResult();

                fileCount++;
                fileListAndIndex.Add(new Tuple<int, string>(fileCount, formatVideoOutputPath));
            }

            // concantenate them together into one file            
            var videoPaths = (from file in fileListAndIndex select file.Item2).ToArray();
            Task.Run(async () =>
            {
                var concantenatedVideos = await FFmpeg.Conversions.FromSnippet.Concatenate(videoFinalPath, videoPaths);
                await concantenatedVideos.Start();
                IMediaInfo finalVideoInfo = await FFmpeg.GetMediaInfo(videoFinalPath);
                finalDuration = finalVideoInfo.Duration;
            }).GetAwaiter().GetResult();


            // assert
            Assert.IsTrue(File.Exists(videoFinalPath));
            Assert.IsTrue(finalDuration >= calculatedDuration);
        }
    }
}
