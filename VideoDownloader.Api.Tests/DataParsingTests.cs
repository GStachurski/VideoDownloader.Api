using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using VideoDownloader.Api.Models;
using VideoDownloader.Api.Options;
using VideoDownloader.Api.Services;
using Moq;

namespace VideoDownloader.Api.Tests
{
    [TestClass]
    public class DataParsingTests
    {
        [TestMethod]
        public void VideoEditWindowsAreParseable()
        {
            // arrange
            var mockOptions = new Mock<IOptions<ApiOptions>>();
            var dataParser = new DataParsingService(mockOptions.Object);

            var downloadList_singleItem = new Download 
            { 
                Name = "Wilderness Cabin", 
                Url = "https://www.youtube.com/watch?v=GWehiacnd1E", 
                EditTimes = "15:41-20:11",
                EditOrder = 1,
                Source = DownloadSource.YouTube
            };

            var downloadList_DoubleItem = new Download 
            { 
                Name = "Winter Night", 
                Url = "https://www.youtube.com/watch?v=8ysFkNYwhAE", 
                EditTimes = "9:53-16:02,17:41-18:20",
                EditOrder = 2,
                Source = DownloadSource.YouTube
            };            

            // act
            var listOneResult = dataParser.GetVideoEditWindows(downloadList_singleItem);
            var listTwoResult = dataParser.GetVideoEditWindows(downloadList_DoubleItem);            

            // assert
            Assert.AreEqual(listOneResult.Count, 1);
            Assert.AreEqual(listTwoResult.Count, 2);
        }
    }
}
