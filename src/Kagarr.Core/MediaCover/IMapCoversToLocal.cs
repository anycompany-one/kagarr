using System.Collections.Generic;
using Kagarr.Core.Games;

namespace Kagarr.Core.MediaCovers
{
    public interface IMapCoversToLocal
    {
        void ConvertToLocalUrls(int gameId, IEnumerable<Games.MediaCover> covers);
        string GetCoverPath(int gameId, MediaCoverTypes coverType);
    }
}
