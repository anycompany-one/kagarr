using System;
using System.Collections.Generic;
using Kagarr.Core.Datastore;
using Kagarr.Core.Games;
using Kagarr.Core.Platforms;

namespace Kagarr.Core.Wishlist
{
    public class WishlistItem : ModelBase
    {
        public WishlistItem()
        {
            Genres = new List<string>();
            Images = new List<MediaCover>();
        }

        public string Title { get; set; }
        public int Year { get; set; }
        public string Overview { get; set; }
        public int IgdbId { get; set; }
        public int? SteamAppId { get; set; }
        public GamePlatform Platform { get; set; }
        public List<string> Genres { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<MediaCover> Images { get; set; }
        public decimal? PriceThreshold { get; set; }
        public bool NotifyOnAnyDeal { get; set; }
        public bool AutoSearch { get; set; }
        public DateTime Added { get; set; }

        public override string ToString()
        {
            return $"[Wishlist][{IgdbId}][{Title ?? "Unknown"}]";
        }
    }
}
