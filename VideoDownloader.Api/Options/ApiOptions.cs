namespace VideoDownloader.Api.Options
{
    public class ApiOptions 
    {
        public VideoSettings VideoSettings { get; set; }
        public int EditWindowLimit { get; set; }
        public bool CheckForHttps { get; set; }
    }

    public class VideoSettings
    {
        public int DownloadTimeout { get; set; }
        public string DownloadPath { get; set; }
        public string FFmpegPath { get; set; }
        public string FFmpegDirectory { get; set; }
    }
}
