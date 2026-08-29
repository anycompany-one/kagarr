using System.Collections.Generic;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.Games
{
    public interface IGameFileRepository : IBasicRepository<GameFile>
    {
        List<GameFile> GetFilesByGame(int gameId);
    }
}
