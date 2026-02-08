using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Http.REST
{
    [ApiController]
    public abstract class RestController<TResource> : Controller
        where TResource : RestResource, new()
    {
        [HttpGet("{id:int}")]
        public virtual ActionResult<TResource> GetResourceById(int id)
        {
            return GetResourceByIdInternal(id);
        }

        protected abstract TResource GetResourceByIdInternal(int id);
    }
}
