using System.Linq;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.Wishlist
{
    public class WishlistRepository : BasicRepository<WishlistItem>, IWishlistRepository
    {
        public WishlistRepository(IDatabase database)
            : base(database)
        {
        }

        public WishlistItem FindByIgdbId(int igdbId)
        {
            return Query(w => w.IgdbId == igdbId).SingleOrDefault();
        }
    }
}
