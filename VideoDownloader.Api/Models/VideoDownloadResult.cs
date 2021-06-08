namespace VideoDownloader.Api.Models
{
    public class VideoDownloadResult
    {
        public bool IsSuccessful { get; set; }
        public string Location { get; set; }
        public double Size { get; set; }
    }
}
