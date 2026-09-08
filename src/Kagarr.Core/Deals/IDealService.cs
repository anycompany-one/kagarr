using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kagarr.Core.Deals
{
    public interface IDealService
    {
        Task<DealSnapshot> CheckDealsAsync(int wishlistItemId);
        Task<List<DealSnapshot>> CheckAllDealsAsync();
        DealSnapshot GetSnapshot(int wishlistItemId);
        List<DealSnapshot> GetAllSnapshots();
    }
}
