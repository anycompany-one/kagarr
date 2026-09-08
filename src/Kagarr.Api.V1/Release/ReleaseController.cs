using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kagarr.Core.Download;
using Kagarr.Core.Indexers;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.Release
{
    [V1ApiController("release")]
    public class ReleaseController : Controller
    {
        private readonly IIndexerService _indexerService;
        private readonly IDownloadClientService _downloadClientService;

        public ReleaseController(IIndexerService indexerService, IDownloadClientService downloadClientService)
        {
            _indexerService = indexerService;
            _downloadClientService = downloadClientService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReleaseResource>>> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return new List<ReleaseResource>();
            }

            var releases = await _indexerService.SearchAllIndexersAsync(term);
            return releases.Select(ReleaseResource.FromModel).ToList();
        }

        [HttpPost]
        public async Task<ActionResult> Grab([FromBody] ReleaseResource resource)
        {
            var release = resource.ToModel();
            var downloadId = await _downloadClientService.SendToDownloadClientAsync(
                release,
                resource.GameId ?? 0,
                resource.GameTitle ?? release.Title);

            return Ok(new { downloadId });
        }
    }
}
