namespace VideoDownloader.Api.Models
{
    public class Download
    {
        public int Id { get; set;  }
        public string Name { get; set; }
        public string Url { get; set; }
        public string EditTimes { get; set; }
        public DownloadSource Source { get; set; }
    }
}
