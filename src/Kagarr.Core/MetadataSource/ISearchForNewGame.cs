using System.Collections.Generic;
using System.Threading.Tasks;
using Kagarr.Core.Games;

namespace Kagarr.Core.MetadataSource
{
    public interface ISearchForNewGame
    {
        Task<List<Game>> SearchForNewGameAsync(string term);
        Task<Game> GetGameInfoAsync(int igdbId);
    }
}
