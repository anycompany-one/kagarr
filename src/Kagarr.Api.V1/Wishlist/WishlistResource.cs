using System;
using System.Collections.Generic;
using Kagarr.Core.Games;
using Kagarr.Core.Platforms;
using Kagarr.Core.Wishlist;
using Kagarr.Http.REST;

namespace Kagarr.Api.V1.Wishlist
{
    public class WishlistResource : RestResource
    {
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
        public string RemoteCover { get; set; }
        public decimal? PriceThreshold { get; set; }
        public bool NotifyOnAnyDeal { get; set; }
        public bool AutoSearch { get; set; }
        public DateTime Added { get; set; }

        // Deal info (populated from snapshot)
        public decimal? CurrentLowestPrice { get; set; }
        public string CurrentLowestStore { get; set; }
        public DateTime? LastDealCheck { get; set; }

        public static WishlistResource FromModel(WishlistItem item)
        {
            if (item == null)
            {
                return null;
            }

            var resource = new WishlistResource
            {
                Id = item.Id,
                Title = item.Title,
                Year = item.Year,
                Overview = item.Overview,
                IgdbId = item.IgdbId,
                SteamAppId = item.SteamAppId,
                Platform = item.Platform,
                Genres = item.Genres,
                Developer = item.Developer,
                Publisher = item.Publisher,
                ReleaseDate = item.ReleaseDate,
                Images = item.Images,
                PriceThreshold = item.PriceThreshold,
                NotifyOnAnyDeal = item.NotifyOnAnyDeal,
                AutoSearch = item.AutoSearch,
                Added = item.Added
            };

            var cover = item.Images?.Find(i => i.CoverType == MediaCoverTypes.Cover);
            if (cover != null)
            {
                resource.RemoteCover = cover.RemoteUrl;
            }

            return resource;
        }

        public WishlistItem ToModel()
        {
            return new WishlistItem
            {
                Id = Id,
                Title = Title,
                Year = Year,
                Overview = Overview,
                IgdbId = IgdbId,
                SteamAppId = SteamAppId,
                Platform = Platform,
                Genres = Genres ?? new List<string>(),
                Developer = Developer,
                Publisher = Publisher,
                ReleaseDate = ReleaseDate,
                Images = Images ?? new List<MediaCover>(),
                PriceThreshold = PriceThreshold,
                NotifyOnAnyDeal = NotifyOnAnyDeal,
                AutoSearch = AutoSearch,
                Added = Added
            };
        }
    }
}
