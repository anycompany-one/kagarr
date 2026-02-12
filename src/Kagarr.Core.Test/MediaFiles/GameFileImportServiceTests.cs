using FluentAssertions;
using Kagarr.Core.Games;
using Kagarr.Core.MediaFiles;
using Kagarr.Core.Platforms;
using Moq;
using NUnit.Framework;

namespace Kagarr.Core.Test.MediaFiles
{
    [TestFixture]
    public class GameFileImportServiceTests
    {
        private Mock<IGameService> _gameService;
        private Mock<IDiskTransferService> _diskTransferService;
        private GameFileImportService _importService;

        [SetUp]
        public void Setup()
        {
            _gameService = new Mock<IGameService>();
            _diskTransferService = new Mock<IDiskTransferService>();
            _importService = new GameFileImportService(_gameService.Object, _diskTransferService.Object);
        }

        [Test]
        public void Import_with_nonexistent_game_should_return_error()
        {
            _gameService.Setup(s => s.GetGame(999))
                .Throws(new Kagarr.Core.Datastore.ModelNotFoundException(typeof(Game), 999));

            var result = _importService.Import("/downloads/test.iso", 999);

            result.Success.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
        }

        [Test]
        public void Import_with_nonexistent_file_should_return_error()
        {
            _gameService.Setup(s => s.GetGame(1))
                .Returns(new Game
                {
                    Id = 1,
                    Title = "Test Game",
                    RootFolderPath = "/games",
                    Platform = GamePlatform.PC
                });

            var result = _importService.Import("/nonexistent/path/game.iso", 1);

            result.Success.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("not found"));
        }

        [Test]
        public void ScanForGameFiles_with_nonexistent_path_should_return_empty_list()
        {
            var result = _importService.ScanForGameFiles("/does/not/exist/anywhere");

            result.Should().BeEmpty();
        }
    }
}
