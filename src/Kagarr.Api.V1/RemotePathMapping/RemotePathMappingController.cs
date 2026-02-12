using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.RemotePathMappings;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.RemotePathMapping
{
    [V1ApiController("remotepathmapping")]
    public class RemotePathMappingController : Controller
    {
        private readonly IRemotePathMappingService _remotePathMappingService;

        public RemotePathMappingController(IRemotePathMappingService remotePathMappingService)
        {
            _remotePathMappingService = remotePathMappingService;
        }

        [HttpGet]
        public ActionResult<List<RemotePathMappingResource>> GetAll()
        {
            return _remotePathMappingService.All().Select(RemotePathMappingResource.FromModel).ToList();
        }

        [HttpGet("{id:int}")]
        public ActionResult<RemotePathMappingResource> Get(int id)
        {
            return RemotePathMappingResource.FromModel(_remotePathMappingService.Get(id));
        }

        [HttpPost]
        public ActionResult<RemotePathMappingResource> Create([FromBody] RemotePathMappingResource resource)
        {
            var model = resource.ToModel();
            var added = _remotePathMappingService.Add(model);
            return Created(
                new global::System.Uri($"/api/v1/remotepathmapping/{added.Id}", global::System.UriKind.Relative),
                RemotePathMappingResource.FromModel(added));
        }

        [HttpPut("{id:int}")]
        public ActionResult<RemotePathMappingResource> Update(int id, [FromBody] RemotePathMappingResource resource)
        {
            var model = resource.ToModel();
            model.Id = id;
            return RemotePathMappingResource.FromModel(_remotePathMappingService.Update(model));
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            _remotePathMappingService.Delete(id);
            return Ok();
        }
    }
}
