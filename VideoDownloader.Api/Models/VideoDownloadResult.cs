namespace VideoDownloader.Api.Models
{
    public class VideoDownloadResult
    {
        public string Title { get; set; }
        public bool IsSuccessful { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
    }
}