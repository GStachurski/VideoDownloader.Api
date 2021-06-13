using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeExplode;
using VideoDownloader.Api.Models;
using System.IO;
using System.Threading.Tasks;

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
                            .SetPreset(ConversionPreset.Fast)
                            .SetFFmpegPath(ffMpegPath)
                            .Build());
            }).GetAwaiter().GetResult();

            // assert
            Assert.IsTrue(File.Exists(fullPath));
        }
    }
}
