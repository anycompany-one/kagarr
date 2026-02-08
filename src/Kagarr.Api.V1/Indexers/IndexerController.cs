using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.Indexers;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.Indexers
{
    [V1ApiController("indexer")]
    public class IndexerController : Controller
    {
        private readonly IIndexerService _indexerService;

        public IndexerController(IIndexerService indexerService)
        {
            _indexerService = indexerService;
        }

        [HttpGet]
        public ActionResult<List<IndexerResource>> GetAll()
        {
            return _indexerService.All().Select(IndexerResource.FromModel).ToList();
        }

        [HttpGet("{id:int}")]
        public ActionResult<IndexerResource> Get(int id)
        {
            return IndexerResource.FromModel(_indexerService.Get(id));
        }

        [HttpPost]
        public ActionResult<IndexerResource> Create([FromBody] IndexerResource resource)
        {
            var model = resource.ToModel();
            var added = _indexerService.Add(model);
            return Created(new global::System.Uri($"/api/v1/indexer/{added.Id}", global::System.UriKind.Relative), IndexerResource.FromModel(added));
        }

        [HttpPut("{id:int}")]
        public ActionResult<IndexerResource> Update(int id, [FromBody] IndexerResource resource)
        {
            var model = resource.ToModel();
            model.Id = id;
            return IndexerResource.FromModel(_indexerService.Update(model));
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            _indexerService.Delete(id);
            return Ok();
        }
    }
}
