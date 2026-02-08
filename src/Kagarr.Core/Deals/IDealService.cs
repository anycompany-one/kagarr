using System.Collections.Generic;

namespace Kagarr.Core.Deals
{
    public interface IDealService
    {
        DealSnapshot CheckDeals(int wishlistItemId);
        List<DealSnapshot> CheckAllDeals();
        DealSnapshot GetSnapshot(int wishlistItemId);
        List<DealSnapshot> GetAllSnapshots();
    }
}
