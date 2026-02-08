using Kagarr.Core.Datastore;

namespace Kagarr.Core.Games
{
    public interface IGameRepository : IBasicRepository<Game>
    {
        Game FindByIgdbId(int igdbId);
    }
}
