using System.Collections.Generic;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace VideoDownloader.Api.Models
{
    public class VideoDownloadResult
    {
        public string Title { get; set; }
        public bool IsSuccessful { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
        public int Order { get; set; }
        public IVideo Video { get; set; }
        public IVideoStreamInfo HqVideoStream { get; set; }
        public IEnumerable<EditWindow> EditWindows { get; set; }
    }
}