using Microsoft.Extensions.Options;
using VideoDownloader.Api.Options;
using Xabe.FFmpeg;

namespace VideoDownloader.Api.Services
{
    public class MpegService
    {
        public ApiOptions _apiOptions;
        public MpegService(IOptions<ApiOptions> apiOptions)
        {
            _apiOptions = apiOptions.Value;
        }
        //public AudioStream ConvertStreamInfoToStream(YoutubeExplode.Videos.Streams.IStreamInfo streamInfo)
        //{       


        //}
    }
}
