using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kagarr.Core.Deals
{
    public interface IDealSource
    {
        string Name { get; }
        Task<List<GameDeal>> GetDealsAsync(string gameTitle, int? steamAppId);
    }
}
