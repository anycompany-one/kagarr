using System;
using System.IO;
using System.Linq;
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
        private Mock<IGameFileRepository> _gameFileRepository;
        private GameFileImportService _importService;
        private string _tempDir;

        [SetUp]
        public void Setup()
        {
            _gameService = new Mock<IGameService>();
            _diskTransferService = new Mock<IDiskTransferService>();
            _gameFileRepository = new Mock<IGameFileRepository>();
            _importService = new GameFileImportService(
                _gameService.Object,
                _diskTransferService.Object,
                _gameFileRepository.Object);

            _tempDir = Path.Combine(Path.GetTempPath(), "kagarr_import_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
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

        [Test]
        public void Import_should_insert_game_file_and_link_it_to_game()
        {
            var game = CreateGame();
            _gameService.Setup(s => s.GetGame(1)).Returns(game);

            var source = CreateSourceFile("game.iso", "iso content");
            SetupTransferToCopy();
            _gameFileRepository.Setup(r => r.Insert(It.IsAny<GameFile>()))
                .Returns<GameFile>(f =>
                {
                    f.Id = 42;
                    return f;
                });

            var result = _importService.Import(source, 1);

            result.Success.Should().BeTrue();
            _gameFileRepository.Verify(r => r.Insert(It.Is<GameFile>(f => f.GameId == 1)), Times.Once);
            game.GameFileId.Should().Be(42);
            _gameService.Verify(s => s.UpdateGame(It.Is<Game>(g => g.GameFileId == 42)), Times.Once);
        }

        [Test]
        public void ImportFolder_with_multiple_files_should_use_unique_destination_paths()
        {
            var game = CreateGame();
            _gameService.Setup(s => s.GetGame(1)).Returns(game);

            var sourceFolder = Path.Combine(_tempDir, "source");
            Directory.CreateDirectory(sourceFolder);
            File.WriteAllText(Path.Combine(sourceFolder, "disc1.iso"), "disc one");
            File.WriteAllText(Path.Combine(sourceFolder, "disc2.iso"), "disc two");

            SetupTransferToCopy();
            _gameFileRepository.Setup(r => r.Insert(It.IsAny<GameFile>())).Returns<GameFile>(f => f);

            var results = _importService.ImportFolder(sourceFolder, 1);

            results.Should().HaveCount(2);
            results.Should().OnlyContain(r => r.Success);

            var destinations = results.Select(r => r.DestinationPath).ToList();
            destinations.Distinct().Should().HaveCount(2, "each imported file must get its own destination");
        }

        [Test]
        public void ImportFolder_with_single_file_should_use_standard_name()
        {
            var game = CreateGame();
            _gameService.Setup(s => s.GetGame(1)).Returns(game);

            var sourceFolder = Path.Combine(_tempDir, "source");
            Directory.CreateDirectory(sourceFolder);
            File.WriteAllText(Path.Combine(sourceFolder, "some.release-GROUP.iso"), "content");

            SetupTransferToCopy();
            _gameFileRepository.Setup(r => r.Insert(It.IsAny<GameFile>())).Returns<GameFile>(f => f);

            var results = _importService.ImportFolder(sourceFolder, 1);

            results.Should().HaveCount(1);
            results[0].Success.Should().BeTrue();
            Path.GetFileName(results[0].DestinationPath).Should().Be("Test Game (2020) [PC].iso");
        }

        private Game CreateGame()
        {
            return new Game
            {
                Id = 1,
                Title = "Test Game",
                Year = 2020,
                RootFolderPath = Path.Combine(_tempDir, "games"),
                Platform = GamePlatform.PC
            };
        }

        private string CreateSourceFile(string name, string content)
        {
            var path = Path.Combine(_tempDir, name);
            File.WriteAllText(path, content);
            return path;
        }

        private void SetupTransferToCopy()
        {
            _diskTransferService
                .Setup(d => d.TransferFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransferMode>()))
                .Returns<string, string, TransferMode>((source, target, mode) =>
                {
                    File.Copy(source, target, false);
                    return TransferMode.Copy;
                });
        }
    }
}
