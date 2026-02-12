using System.Collections.Generic;
using System.Linq;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.History;
using NLog;

namespace Kagarr.Core.Games
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public GameService(IGameRepository gameRepository, IHistoryService historyService)
        {
            _gameRepository = gameRepository;
            _historyService = historyService;
            _logger = KagarrLogger.GetLogger(this);
        }

        public Game GetGame(int id)
        {
            return _gameRepository.Get(id);
        }

        public List<Game> GetAllGames()
        {
            return _gameRepository.All().ToList();
        }

        public Game AddGame(Game game)
        {
            _logger.Info("Adding game '{0}' (IGDB: {1})", game.Title, game.IgdbId);
            game.CleanTitle = game.Title?.ToLowerInvariant().Replace(" ", "");
            game.SortTitle = game.Title;
            game.Added = System.DateTime.UtcNow;
            return _gameRepository.Insert(game);
        }

        public Game UpdateGame(Game game)
        {
            _logger.Info("Updating game '{0}'", game.Title);
            return _gameRepository.Update(game);
        }

        public void DeleteGame(int id)
        {
            _logger.Info("Deleting game with ID {0}", id);

            try
            {
                var game = _gameRepository.Get(id);
                _historyService.RecordEvent(
                    HistoryEventType.Deleted,
                    id,
                    game.Title,
                    null,
                    "Removed from library");
            }
            catch
            {
                // Game may already be gone — still delete
            }

            _gameRepository.Delete(id);
        }

        public Game FindByIgdbId(int igdbId)
        {
            return _gameRepository.FindByIgdbId(igdbId);
        }
    }
}
