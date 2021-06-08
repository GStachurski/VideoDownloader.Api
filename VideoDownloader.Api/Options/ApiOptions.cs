namespace VideoDownloader.Api.Options
{
    public class ApiOptions
    {
        public string VideoDownloadUrl { get; }
        public string VideoEditingUrl { get; }
        public string VideoFFmpegPath { get; }
        public bool CheckForHttps { get; }
    }
}
