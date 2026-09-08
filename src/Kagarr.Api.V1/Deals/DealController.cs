using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kagarr.Core.Deals;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.Deals
{
    [V1ApiController("deal")]
    public class DealController : Controller
    {
        private readonly IDealService _dealService;

        public DealController(IDealService dealService)
        {
            _dealService = dealService;
        }

        [HttpGet]
        public ActionResult<List<DealResource>> GetAll()
        {
            return _dealService.GetAllSnapshots()
                .Select(DealResource.FromModel)
                .ToList();
        }

        [HttpGet("{wishlistItemId:int}")]
        public ActionResult<DealResource> GetDeals(int wishlistItemId)
        {
            var snapshot = _dealService.GetSnapshot(wishlistItemId);
            if (snapshot == null)
            {
                return NotFound();
            }

            return DealResource.FromModel(snapshot);
        }

        [HttpPost("{wishlistItemId:int}/check")]
        public async Task<ActionResult<DealResource>> CheckDeals(int wishlistItemId)
        {
            var snapshot = await _dealService.CheckDealsAsync(wishlistItemId);
            return DealResource.FromModel(snapshot);
        }

        [HttpPost("check")]
        public async Task<ActionResult<List<DealResource>>> CheckAllDeals()
        {
            var snapshots = await _dealService.CheckAllDealsAsync();
            return snapshots
                .Select(DealResource.FromModel)
                .ToList();
        }
    }
}
