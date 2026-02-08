using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.Download;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.DownloadClient
{
    [V1ApiController("downloadclient")]
    public class DownloadClientController : Controller
    {
        private readonly IDownloadClientService _downloadClientService;

        public DownloadClientController(IDownloadClientService downloadClientService)
        {
            _downloadClientService = downloadClientService;
        }

        [HttpGet]
        public ActionResult<List<DownloadClientResource>> GetAll()
        {
            return _downloadClientService.All().Select(DownloadClientResource.FromModel).ToList();
        }

        [HttpGet("{id:int}")]
        public ActionResult<DownloadClientResource> Get(int id)
        {
            return DownloadClientResource.FromModel(_downloadClientService.Get(id));
        }

        [HttpPost]
        public ActionResult<DownloadClientResource> Create([FromBody] DownloadClientResource resource)
        {
            var model = resource.ToModel();
            var added = _downloadClientService.Add(model);
            return Created(new global::System.Uri($"/api/v1/downloadclient/{added.Id}", global::System.UriKind.Relative), DownloadClientResource.FromModel(added));
        }

        [HttpPut("{id:int}")]
        public ActionResult<DownloadClientResource> Update(int id, [FromBody] DownloadClientResource resource)
        {
            var model = resource.ToModel();
            model.Id = id;
            return DownloadClientResource.FromModel(_downloadClientService.Update(model));
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            _downloadClientService.Delete(id);
            return Ok();
        }
    }
}
