using Kagarr.Core.Datastore;

namespace Kagarr.Core.Wishlist
{
    public interface IWishlistRepository : IBasicRepository<WishlistItem>
    {
        WishlistItem FindByIgdbId(int igdbId);
    }
}
