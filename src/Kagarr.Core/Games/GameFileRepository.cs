using System.Collections.Generic;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.Games
{
    public class GameFileRepository : BasicRepository<GameFile>, IGameFileRepository
    {
        public GameFileRepository(IDatabase database)
            : base(database)
        {
        }

        public List<GameFile> GetFilesByGame(int gameId)
        {
            return Query(f => f.GameId == gameId);
        }
    }
}
