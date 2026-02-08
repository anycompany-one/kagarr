using Kagarr.Core.Datastore;

namespace Kagarr.Core.Deals
{
    public interface IDealSnapshotRepository : IBasicRepository<DealSnapshot>
    {
        DealSnapshot FindByWishlistItemId(int wishlistItemId);
    }
}
