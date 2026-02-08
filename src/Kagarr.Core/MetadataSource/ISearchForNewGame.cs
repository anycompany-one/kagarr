using System.Collections.Generic;
using Kagarr.Core.Games;

namespace Kagarr.Core.MetadataSource
{
    public interface ISearchForNewGame
    {
        List<Game> SearchForNewGame(string term);
        Game GetGameInfo(int igdbId);
    }
}
