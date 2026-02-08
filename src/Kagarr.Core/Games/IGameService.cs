using System.Collections.Generic;

namespace Kagarr.Core.Games
{
    public interface IGameService
    {
        Game GetGame(int id);
        List<Game> GetAllGames();
        Game AddGame(Game game);
        Game UpdateGame(Game game);
        void DeleteGame(int id);
        Game FindByIgdbId(int igdbId);
    }
}
