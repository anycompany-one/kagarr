using System.Linq;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.Deals
{
    public class DealSnapshotRepository : BasicRepository<DealSnapshot>, IDealSnapshotRepository
    {
        public DealSnapshotRepository(IDatabase database)
            : base(database)
        {
        }

        public DealSnapshot FindByWishlistItemId(int wishlistItemId)
        {
            return Query(d => d.WishlistItemId == wishlistItemId).SingleOrDefault();
        }
    }
}
