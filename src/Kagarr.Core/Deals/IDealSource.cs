using System.Collections.Generic;

namespace Kagarr.Core.Deals
{
    public interface IDealSource
    {
        string Name { get; }
        List<GameDeal> GetDeals(string gameTitle, int? steamAppId);
    }
}
