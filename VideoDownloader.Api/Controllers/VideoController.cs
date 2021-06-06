using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
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
        /// Takes a recommendation search request and indexes it's elastic search query document output to the percolator index.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [SwaggerResponse((int)HttpStatusCode.OK, "Successful percolate query index.")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Bad Request, see error output.")]
        [HttpPost("downloadandedit")]
        public async Task<ActionResult> DownloadAndEdit([FromBody] List<Download> downloads)
        {
            return Ok();
        }
    }
}
