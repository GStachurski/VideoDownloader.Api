using System;

namespace VideoDownloader.Api.Models
{
    public class VideoEditResult
    {
        public string Location { get; set; }
        public int Order { get; set; }
        public long Size { get; set; }
        public TimeSpan Length { get; set; }
    }
}
