using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
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
        private readonly IVideoDownloadService  _videoService;

        public VideoController(IVideoDownloadService videoService)
        {
            _videoService = videoService;
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
        public async Task<ActionResult> DownloadAndEdit([FromBody] List<Download> downloads)
        {
            try
            {
                if (downloads.Any())
                {
                    var videos = await _videoService.GetVideos(downloads);
                    //var results = await _videoService.GetDownloads(videos);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "error occured while geting video manifests");
                return BadRequest();
            }

            return Ok();
        }
    }
}
