using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kagarr.Core.Download;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.Queue
{
    [V1ApiController("queue")]
    public class QueueController : Controller
    {
        private readonly IDownloadClientService _downloadClientService;

        public QueueController(IDownloadClientService downloadClientService)
        {
            _downloadClientService = downloadClientService;
        }

        [HttpGet]
        public async Task<ActionResult<List<QueueResource>>> GetQueue()
        {
            var queue = await _downloadClientService.GetQueueAsync();
            return queue.Select(QueueResource.FromModel).ToList();
        }
    }
}
