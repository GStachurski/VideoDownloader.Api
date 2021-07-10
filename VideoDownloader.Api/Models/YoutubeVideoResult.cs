using System.Collections.Generic;
using YoutubeExplode.Videos;

namespace VideoDownloader.Api.Models
{
    public class YoutubeVideoResult
    {
        public Video Video { get; set; }
        public List<EditWindow> EditWindows { get; set; }
        public int Order { get; set; }
    }
}
