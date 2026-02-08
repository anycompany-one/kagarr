using System.Linq;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.Games
{
    public class GameRepository : BasicRepository<Game>, IGameRepository
    {
        public GameRepository(IDatabase database)
            : base(database)
        {
        }

        public Game FindByIgdbId(int igdbId)
        {
            return Query(g => g.IgdbId == igdbId).SingleOrDefault();
        }
    }
}
