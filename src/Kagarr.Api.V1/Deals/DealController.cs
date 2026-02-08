using System.Collections.Generic;
using System.Linq;
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
        public ActionResult<DealResource> CheckDeals(int wishlistItemId)
        {
            var snapshot = _dealService.CheckDeals(wishlistItemId);
            return DealResource.FromModel(snapshot);
        }

        [HttpPost("check")]
        public ActionResult<List<DealResource>> CheckAllDeals()
        {
            return _dealService.CheckAllDeals()
                .Select(DealResource.FromModel)
                .ToList();
        }
    }
}
