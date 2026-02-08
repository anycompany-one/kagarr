using System;
using System.Collections.Generic;
using System.Linq;
using Kagarr.Common.Instrumentation;
using NLog;

namespace Kagarr.Core.Wishlist
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly Logger _logger;

        public WishlistService(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
            _logger = KagarrLogger.GetLogger(this);
        }

        public WishlistItem GetItem(int id)
        {
            return _wishlistRepository.Get(id);
        }

        public List<WishlistItem> GetAll()
        {
            return _wishlistRepository.All().ToList();
        }

        public WishlistItem Add(WishlistItem item)
        {
            var existing = _wishlistRepository.FindByIgdbId(item.IgdbId);
            if (existing != null)
            {
                _logger.Warn("Game '{0}' (IGDB: {1}) is already on the wishlist", item.Title, item.IgdbId);
                return existing;
            }

            _logger.Info("Adding '{0}' to wishlist", item.Title);
            item.Added = DateTime.UtcNow;
            return _wishlistRepository.Insert(item);
        }

        public WishlistItem Update(WishlistItem item)
        {
            _logger.Info("Updating wishlist item '{0}'", item.Title);
            return _wishlistRepository.Update(item);
        }

        public void Delete(int id)
        {
            _logger.Info("Removing wishlist item with ID {0}", id);
            _wishlistRepository.Delete(id);
        }

        public WishlistItem FindByIgdbId(int igdbId)
        {
            return _wishlistRepository.FindByIgdbId(igdbId);
        }
    }
}
