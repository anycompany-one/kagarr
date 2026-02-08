using System.Collections.Generic;

namespace Kagarr.Core.Wishlist
{
    public interface IWishlistService
    {
        WishlistItem GetItem(int id);
        List<WishlistItem> GetAll();
        WishlistItem Add(WishlistItem item);
        WishlistItem Update(WishlistItem item);
        void Delete(int id);
        WishlistItem FindByIgdbId(int igdbId);
    }
}
