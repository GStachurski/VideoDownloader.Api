using System;

namespace VideoDownloader.Api.Models
{
    public class EditWindow
    {
        // MM:ss
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
