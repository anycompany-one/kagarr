using System;
using System.Collections.Generic;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.Deals
{
    public class DealSnapshot : ModelBase
    {
        public DealSnapshot()
        {
            Deals = new List<GameDeal>();
        }

        public int WishlistItemId { get; set; }
        public int IgdbId { get; set; }
        public string GameTitle { get; set; }
        public decimal? LowestPrice { get; set; }
        public string LowestPriceStore { get; set; }
        public DateTime LastChecked { get; set; }
        public List<GameDeal> Deals { get; set; }
    }
}
