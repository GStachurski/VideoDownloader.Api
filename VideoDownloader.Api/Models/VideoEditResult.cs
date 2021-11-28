using System;
using Xabe.FFmpeg;

namespace VideoDownloader.Api.Models
{
    public class VideoEditResult
    {
        public string Location { get; set; }
        public int Order { get; set; }
        public long Size { get; set; }
        public double Framerate { get; set; } 
        public TimeSpan Length { get; set; }
        public IMediaInfo MediaInfo { get; set; }
    }
}