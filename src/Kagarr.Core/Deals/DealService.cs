using System;
using System.Collections.Generic;
using System.Linq;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Notifications;
using Kagarr.Core.Wishlist;
using NLog;

namespace Kagarr.Core.Deals
{
    public class DealService : IDealService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IDealSnapshotRepository _snapshotRepository;
        private readonly IEnumerable<IDealSource> _dealSources;
        private readonly INotificationService _notificationService;
        private readonly Logger _logger;

        public DealService(
            IWishlistRepository wishlistRepository,
            IDealSnapshotRepository snapshotRepository,
            IEnumerable<IDealSource> dealSources,
            INotificationService notificationService)
        {
            _wishlistRepository = wishlistRepository;
            _snapshotRepository = snapshotRepository;
            _dealSources = dealSources;
            _notificationService = notificationService;
            _logger = KagarrLogger.GetLogger(this);
        }

        public DealSnapshot CheckDeals(int wishlistItemId)
        {
            var item = _wishlistRepository.Get(wishlistItemId);
            return CheckDealsForItem(item);
        }

        public List<DealSnapshot> CheckAllDeals()
        {
            var items = _wishlistRepository.All().ToList();
            _logger.Info("Checking deals for {0} wishlisted games", items.Count);

            var results = new List<DealSnapshot>();
            foreach (var item in items)
            {
                var snapshot = CheckDealsForItem(item);
                if (snapshot != null)
                {
                    results.Add(snapshot);
                }
            }

            return results;
        }

        public DealSnapshot GetSnapshot(int wishlistItemId)
        {
            return _snapshotRepository.FindByWishlistItemId(wishlistItemId);
        }

        public List<DealSnapshot> GetAllSnapshots()
        {
            return _snapshotRepository.All().ToList();
        }

        private DealSnapshot CheckDealsForItem(WishlistItem item)
        {
            _logger.Debug("Checking deals for '{0}'", item.Title);

            var allDeals = new List<GameDeal>();

            foreach (var source in _dealSources)
            {
                try
                {
                    var deals = source.GetDeals(item.Title, item.SteamAppId);
                    allDeals.AddRange(deals);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Deal source '{0}' failed for '{1}'", source.Name, item.Title);
                }
            }

            var existing = _snapshotRepository.FindByWishlistItemId(item.Id);
            var previousLowest = existing?.LowestPrice;

            var lowestDeal = allDeals.OrderBy(d => d.CurrentPrice).FirstOrDefault();

            var snapshot = existing ?? new DealSnapshot();
            snapshot.WishlistItemId = item.Id;
            snapshot.IgdbId = item.IgdbId;
            snapshot.GameTitle = item.Title;
            snapshot.LastChecked = DateTime.UtcNow;
            snapshot.Deals = allDeals;

            if (lowestDeal != null)
            {
                snapshot.LowestPrice = lowestDeal.CurrentPrice;
                snapshot.LowestPriceStore = lowestDeal.Store;
            }

            if (existing != null)
            {
                _snapshotRepository.Update(snapshot);
            }
            else
            {
                _snapshotRepository.Insert(snapshot);
            }

            // Fire notification if price dropped below threshold
            if (lowestDeal != null && ShouldNotify(item, lowestDeal, previousLowest))
            {
                _notificationService.OnDealFound(item, lowestDeal);
            }

            return snapshot;
        }

        private static bool ShouldNotify(WishlistItem item, GameDeal deal, decimal? previousLowest)
        {
            if (item.NotifyOnAnyDeal && deal.DiscountPercent > 0)
            {
                // Only notify if price actually changed
                return previousLowest == null || deal.CurrentPrice < previousLowest.Value;
            }

            if (item.PriceThreshold.HasValue && deal.CurrentPrice <= item.PriceThreshold.Value)
            {
                return previousLowest == null || deal.CurrentPrice < previousLowest.Value;
            }

            return false;
        }
    }
}
