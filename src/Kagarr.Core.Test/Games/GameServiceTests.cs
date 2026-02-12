using System.Collections.Generic;
using FluentAssertions;
using Kagarr.Core.Games;
using Kagarr.Core.History;
using Kagarr.Core.Platforms;
using Moq;
using NUnit.Framework;

namespace Kagarr.Core.Test.Games
{
    [TestFixture]
    public class GameServiceTests
    {
        private Mock<IGameRepository> _gameRepository;
        private Mock<IHistoryService> _historyService;
        private GameService _gameService;

        [SetUp]
        public void Setup()
        {
            _gameRepository = new Mock<IGameRepository>();
            _historyService = new Mock<IHistoryService>();
            _gameService = new GameService(_gameRepository.Object, _historyService.Object);
        }

        [Test]
        public void GetAllGames_should_return_all_games()
        {
            var games = new List<Game>
            {
                new Game { Id = 1, Title = "Baldur's Gate 3", IgdbId = 119171, Platform = GamePlatform.PC },
                new Game { Id = 2, Title = "Elden Ring", IgdbId = 119133, Platform = GamePlatform.PC }
            };

            _gameRepository.Setup(r => r.All()).Returns(games);

            var result = _gameService.GetAllGames();

            result.Should().HaveCount(2);
            result[0].Title.Should().Be("Baldur's Gate 3");
        }

        [Test]
        public void AddGame_should_set_clean_title()
        {
            var game = new Game
            {
                Title = "Baldur's Gate 3",
                IgdbId = 119171,
                Platform = GamePlatform.PC,
                Path = "/games/bg3"
            };

            _gameRepository.Setup(r => r.Insert(It.IsAny<Game>())).Returns<Game>(g =>
            {
                g.Id = 1;
                return g;
            });

            var result = _gameService.AddGame(game);

            result.CleanTitle.Should().Be("baldur'sgate3");
        }

        [Test]
        public void DeleteGame_should_call_repository()
        {
            _gameService.DeleteGame(1);

            _gameRepository.Verify(r => r.Delete(1), Times.Once);
        }
    }
}
