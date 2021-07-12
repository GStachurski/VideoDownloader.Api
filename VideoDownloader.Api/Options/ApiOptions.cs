namespace VideoDownloader.Api.Options
{
    public class ApiOptions 
    {
        public VideoSettings VideoSettings { get; set; }
        public bool CheckForHttps { get; set; }
        public bool SeperateAudioAndVideoStreams { get; set; }
    }

    public class VideoSettings
    {
        public string DownloadPath { get; set; }
        public string DownloadUrl { get; set; }
        public string EditingUrl { get; set; }
        public string FFmpegPath { get; set; }
        public string FFmpegDirectory { get; set; }
    }
}
