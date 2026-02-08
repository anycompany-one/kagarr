using System;
using System.Collections.Generic;
using System.Linq;
using Kagarr.Core.Deals;
using Kagarr.Http.REST;

namespace Kagarr.Api.V1.Deals
{
    public class DealResource : RestResource
    {
        public int WishlistItemId { get; set; }
        public string GameTitle { get; set; }
        public decimal? LowestPrice { get; set; }
        public string LowestPriceStore { get; set; }
        public DateTime LastChecked { get; set; }
        public List<GameDealResource> Deals { get; set; }

        public static DealResource FromModel(DealSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return null;
            }

            return new DealResource
            {
                Id = snapshot.Id,
                WishlistItemId = snapshot.WishlistItemId,
                GameTitle = snapshot.GameTitle,
                LowestPrice = snapshot.LowestPrice,
                LowestPriceStore = snapshot.LowestPriceStore,
                LastChecked = snapshot.LastChecked,
                Deals = snapshot.Deals?.Select(d => new GameDealResource
                {
                    Store = d.Store,
                    Title = d.Title,
                    CurrentPrice = d.CurrentPrice,
                    RegularPrice = d.RegularPrice,
                    DiscountPercent = d.DiscountPercent,
                    CurrencyCode = d.CurrencyCode,
                    DealUrl = d.DealUrl,
                    IsFree = d.IsFree
                }).ToList() ?? new List<GameDealResource>()
            };
        }
    }

    public class GameDealResource
    {
        public string Store { get; set; }
        public string Title { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal RegularPrice { get; set; }
        public int DiscountPercent { get; set; }
        public string CurrencyCode { get; set; }
        public string DealUrl { get; set; }
        public bool IsFree { get; set; }
    }
}
