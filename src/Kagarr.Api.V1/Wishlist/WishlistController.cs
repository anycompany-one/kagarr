using System;
using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.Deals;
using Kagarr.Core.Wishlist;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.Wishlist
{
    [V1ApiController("wishlist")]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly IDealService _dealService;

        public WishlistController(IWishlistService wishlistService, IDealService dealService)
        {
            _wishlistService = wishlistService;
            _dealService = dealService;
        }

        [HttpGet]
        public ActionResult<List<WishlistResource>> GetAll()
        {
            var items = _wishlistService.GetAll();
            var snapshots = _dealService.GetAllSnapshots();
            var snapshotMap = snapshots.ToDictionary(s => s.WishlistItemId);

            return items.Select(item =>
            {
                var resource = WishlistResource.FromModel(item);

                if (snapshotMap.TryGetValue(item.Id, out var snapshot))
                {
                    resource.CurrentLowestPrice = snapshot.LowestPrice;
                    resource.CurrentLowestStore = snapshot.LowestPriceStore;
                    resource.LastDealCheck = snapshot.LastChecked;
                }

                return resource;
            }).ToList();
        }

        [HttpGet("{id:int}")]
        public ActionResult<WishlistResource> GetOne(int id)
        {
            var item = _wishlistService.GetItem(id);
            var resource = WishlistResource.FromModel(item);

            var snapshot = _dealService.GetSnapshot(id);
            if (snapshot != null)
            {
                resource.CurrentLowestPrice = snapshot.LowestPrice;
                resource.CurrentLowestStore = snapshot.LowestPriceStore;
                resource.LastDealCheck = snapshot.LastChecked;
            }

            return resource;
        }

        [HttpPost]
        public ActionResult<WishlistResource> Add([FromBody] WishlistResource resource)
        {
            var item = resource.ToModel();
            var added = _wishlistService.Add(item);
            return Created(new Uri($"/api/v1/wishlist/{added.Id}", UriKind.Relative), WishlistResource.FromModel(added));
        }

        [HttpPut("{id:int}")]
        public ActionResult<WishlistResource> Update(int id, [FromBody] WishlistResource resource)
        {
            var item = resource.ToModel();
            item.Id = id;
            var updated = _wishlistService.Update(item);
            return WishlistResource.FromModel(updated);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            _wishlistService.Delete(id);
            return Ok();
        }
    }
}
