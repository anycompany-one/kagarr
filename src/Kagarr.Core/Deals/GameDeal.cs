using System;

namespace Kagarr.Core.Deals
{
    public class GameDeal
    {
        public string Store { get; set; }
        public string Title { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal RegularPrice { get; set; }
        public int DiscountPercent { get; set; }
        public string CurrencyCode { get; set; }
        public string DealUrl { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsFree { get; set; }
    }
}
