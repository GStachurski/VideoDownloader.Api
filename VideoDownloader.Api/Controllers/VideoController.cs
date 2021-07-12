using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VideoDownloader.Api.Interfaces;
using VideoDownloader.Api.Models;

namespace VideoDownloader.Api.Controllers
{
    [Route("api/video")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService  _videoService;
        private readonly IEditingService _editingService;

        public VideoController(IVideoService videoService, IEditingService editingService)
        {
            _videoService = videoService;
            _editingService = editingService;
        }

        /// <summary>
        /// Takes a list of videos and converts them to downloads
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// {
        ///   "downloads": [
        ///     {
        ///       "Name": "Wilderness Cabin",
        ///       "Url": "https://www.youtube.com/watch?v=GWehiacnd1E",
        ///       "EditTimes": "15:41-20:11"
        ///     },
        ///     {
        ///       "Name": "Roadside Barn",
        ///       "Url": "https://www.youtube.com/watch?v=vJpKhiXvXdA",
        ///       "EditTimes": "16:24-22:00"
        ///     }]
        /// }
        /// </remarks>
        /// <param name="downloads">A JSON array of downloads</param>
        /// <returns></returns>
        [SwaggerResponse((int)HttpStatusCode.OK, "Successful video download.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Bad Request, see error output.")]
        [HttpPost("downloadandedit")]
        public async Task<ActionResult> DownloadAndEdit([FromBody] List<Download> downloads, string fileName)
        {
            var result = new VideoEditResult();

            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("no filename provided");
                }

                if (downloads.Any())
                {
                    Log.Information($"getting {downloads.Count} youtube video manifests");
                    var videos = await _videoService.GetYoutubeVideos(downloads);
                    
                    Log.Information($"downloading {videos.Count()} videos");
                    var files = await _videoService.GetVideoDownloads(videos);
                    
                    Log.Information($"creating {files.Sum(file => file.EditWindows.Count())} video clips");
                    var edits = await _editingService.GetVideoEditResults(files);
                    
                    Log.Information($"creating final video {fileName}.mp4");
                    result = await _editingService.CreateVideoFromVideoClips(edits, fileName);
                }
                else
                {
                    Log.Warning($"no downloads provided"); 
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "error occured while geting video manifests"); 
                return BadRequest();
            }

            return Ok(result);
        }
    }
}
