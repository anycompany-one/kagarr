using System;
using System.Collections.Generic;
using System.Linq;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Games;
using Kagarr.Core.Organizer;
using NLog;

namespace Kagarr.Core.MediaFiles
{
    public class GameFileImportService : IImportGameFile
    {
        private readonly IGameService _gameService;
        private readonly Logger _logger;

        public GameFileImportService(IGameService gameService)
        {
            _gameService = gameService;
            _logger = KagarrLogger.GetLogger(this);
        }

        public ImportResult Import(string sourcePath, int gameId)
        {
            var result = new ImportResult
            {
                SourcePath = sourcePath
            };

            try
            {
                var game = _gameService.GetGame(gameId);
                if (game == null)
                {
                    result.Errors.Add($"Game with ID {gameId} not found");
                    return result;
                }

                result.Game = game;

                if (!global::System.IO.File.Exists(sourcePath))
                {
                    result.Errors.Add($"Source file not found: {sourcePath}");
                    return result;
                }

                var fileType = GameFileDetector.DetectFileType(sourcePath);
                if (fileType == GameFileType.Unknown)
                {
                    result.Errors.Add($"Unrecognized file type: {sourcePath}");
                    return result;
                }

                // Build destination path
                var rootFolder = game.RootFolderPath;
                if (string.IsNullOrEmpty(rootFolder))
                {
                    result.Errors.Add("Game has no root folder path configured");
                    return result;
                }

                var gameFolder = FileNameBuilder.BuildGameFolder(game);
                var gameFolderPath = global::System.IO.Path.Combine(rootFolder, gameFolder);

                global::System.IO.Directory.CreateDirectory(gameFolderPath);

                var fileName = FileNameBuilder.BuildGameFileName(game, sourcePath);
                var destinationPath = global::System.IO.Path.Combine(gameFolderPath, fileName);

                _logger.Info("Importing '{0}' to '{1}'", sourcePath, destinationPath);

                // Move the file
                if (global::System.IO.File.Exists(destinationPath))
                {
                    _logger.Warn("Destination file already exists, removing: {0}", destinationPath);
                    global::System.IO.File.Delete(destinationPath);
                }

                global::System.IO.File.Move(sourcePath, destinationPath);

                // Create GameFile record
                var gameFile = new GameFile
                {
                    GameId = gameId,
                    RelativePath = global::System.IO.Path.GetRelativePath(rootFolder, destinationPath),
                    Size = new global::System.IO.FileInfo(destinationPath).Length,
                    DateAdded = DateTime.UtcNow,
                    Platform = game.Platform
                };

                // Update game path
                game.Path = gameFolderPath;
                _gameService.UpdateGame(game);

                result.Success = true;
                result.ImportedFile = gameFile;
                result.DestinationPath = destinationPath;

                _logger.Info("Successfully imported '{0}' for game '{1}'", fileName, game.Title);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error importing file '{0}'", sourcePath);
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public List<ImportResult> ImportFolder(string folderPath, int gameId)
        {
            _logger.Info("Scanning folder for import: {0}", folderPath);

            var gameFiles = ScanForGameFiles(folderPath);
            _logger.Info("Found {0} game files in '{1}'", gameFiles.Count, folderPath);

            return gameFiles.Select(f => Import(f, gameId)).ToList();
        }

        public List<string> ScanForGameFiles(string path)
        {
            return GameFileDetector.ScanDirectory(path);
        }
    }
}
