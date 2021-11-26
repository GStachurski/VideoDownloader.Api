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

        [TestMethod]
        public void BlocksOfXOrMoreSnippetsAreBrokenUp()
        {
            // arrange
            var snippetMax = 30;
            var snippetBlockMax = 5;
            //var locList = new List<string> {
            //    "F:\\Videos\\Bob Ross - Majestic Mountains (Season 4 Episode 3)_0.mp4",
            //    "F:\\Videos\\Bob Ross - Majestic Mountains (Season 4 Episode 3)_1.mp4",
            //    "F:\\Videos\\Bob Ross - Distant Mountains (Season 14 Episode 1)_2.mp4",
            //    "F:\\Videos\\Bob Ross - Distant Mountains (Season 14 Episode 1)_3.mp4",
            //    "F:\\Videos\\Bob Ross - One Hour Special - The Grandeur of Summer_4.mp4",
            //    "F:\\Videos\\Bob Ross - One Hour Special - The Grandeur of Summer_5.mp4",
            //    "F:\\Videos\\Bob Ross - One Hour Special - The Grandeur of Summer_6.mp4",
            //    "F:\\Videos\\Bob Ross - Frozen Solitude (Season 13 Episode 2)_7.mp4",
            //    "F:\\Videos\\Bob Ross - Frozen Solitude (Season 13 Episode 2)_8.mp4",
            //    "F:\\Videos\\Bob Ross - Frozen Solitude (Season 13 Episode 2)_9.mp4",
            //    "F:\\Videos\\Bob Ross - Frozen Solitude (Season 13 Episode 2)_10.mp4",
            //    "F:\\Videos\\Bob Ross - Frozen Solitude (Season 13 Episode 2)_11.mp4",
            //    "F:\\Videos\\Bob Ross - Foot of the Mountain (Season 8 Episode 8)_12.mp4",
            //    "F:\\Videos\\Bob Ross - Foot of the Mountain (Season 8 Episode 8)_13.mp4",
            //    "F:\\Videos\\Bob Ross - Foot of the Mountain (Season 8 Episode 8)_14.mp4",
            //    "F:\\Videos\\Bob Ross - Bubbling Mountain Brook (Season 8 Episode 6)_15.mp4",
            //    "F:\\Videos\\Bob Ross - Bubbling Mountain Brook (Season 8 Episode 6)_16.mp4",
            //    "F:\\Videos\\Bob Ross - Bubbling Mountain Brook (Season 8 Episode 6)_17.mp4",
            //    "F:\\Videos\\Bob Ross - Bubbling Mountain Brook (Season 8 Episode 6)_18.mp4",
            //    "F:\\Videos\\Bob Ross - Winter Frost (Season 10 Episode 12)_19.mp4",
            //    "F:\\Videos\\Bob Ross - Winter Frost (Season 10 Episode 12)_20.mp4",
            //    "F:\\Videos\\Bob Ross - Winter Frost (Season 10 Episode 12)_21.mp4",
            //    "F:\\Videos\\Bob Ross - Winter Frost (Season 10 Episode 12)_22.mp4",
            //    "F:\\Videos\\Bob Ross - Towering Peaks (Season 10 Episode 1)_23.mp4",
            //    "F:\\Videos\\Bob Ross - Towering Peaks (Season 10 Episode 1)_24.mp4",
            //    "F:\\Videos\\Bob Ross - Towering Peaks (Season 10 Episode 1)_25.mp4",
            //    "F:\\Videos\\Bob Ross - Mighty Mountain Lake (Season 16 Episode 12)_26.mp4",
            //    "F:\\Videos\\Bob Ross - Mighty Mountain Lake (Season 16 Episode 12)_27.mp4",
            //    "F:\\Videos\\Bob Ross - Mighty Mountain Lake (Season 16 Episode 12)_28.mp4",
            //    "F:\\Videos\\Bob Ross - Mystic Mountain (Season 20 Episode 1)_29.mp4",
            //    "F:\\Videos\\Bob Ross - Mystic Mountain (Season 20 Episode 1)_30.mp4",
            //    "F:\\Videos\\Bob Ross - Mystic Mountain (Season 20 Episode 1)_31.mp4",
            //    "F:\\Videos\\Bob Ross - Mystic Mountain (Season 20 Episode 1)_32.mp4"
            //};
            //var editCounts = locList.Count();

            //// setup the final file output location and name
            //var finalFileName = $"{_downloadPath}{fileName}{"_"}{listEdits.Count()}{".mp4"}";
            //var concantVideos = await FFmpeg
            //    .Conversions
            //    .FromSnippet
            //    .Concatenate(finalFileName, (from edit in listEdits select edit.Location).ToArray());
            //var result = await concantVideos
            //    .UseMultiThread(true)
            //    .Start();
            //var info = await FFmpeg.GetMediaInfo(finalFileName);

            //return new VideoEditResult
            //{
            //    Location = finalFileName,
            //    Size = info.Size,
            //    Length = info.Duration
            //};


            // act

            // assert

        }
    }
}
