using System.Collections.Generic;
using System.Linq;
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
        public ActionResult<List<QueueResource>> GetQueue()
        {
            return _downloadClientService.GetQueue().Select(QueueResource.FromModel).ToList();
        }
    }
}
