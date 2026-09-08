using System.Collections.Generic;
using System.Threading.Tasks;
using Kagarr.Core.Games;

namespace Kagarr.Core.MediaCovers
{
    public interface IMapCoversToLocal
    {
        Task ConvertToLocalUrlsAsync(int gameId, IEnumerable<Games.MediaCover> covers);
        string GetCoverPath(int gameId, MediaCoverTypes coverType);

        /// <summary>
        /// Resolves a stored cover file for serving. Returns null when the file name is
        /// invalid (path traversal) or the file does not exist.
        /// </summary>
        string GetStoredCoverPath(int gameId, string fileName);
    }
}
