using Microsoft.AspNetCore.Mvc;
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
        private readonly IVideoDownloadService  _videoService;

        public VideoController(IVideoDownloadService videoService)
        {
            _videoService = videoService;
        }

        /// <summary>
        /// Takes a list of videos and converts them to downloads
        /// </summary>
        /// <param name="request"></param>
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
                    var videos = await _videoService.GetVideosFromDownloads(downloads);
                    var results = await _videoService.GetDownloadResults(videos);
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
