using System.Net.Http;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Games;
using NLog;

namespace Kagarr.Core.MediaCovers
{
    public class MediaCoverService : IMapCoversToLocal
    {
        private readonly Logger _logger;
        private readonly string _coverRootFolder;

        public MediaCoverService()
        {
            _logger = KagarrLogger.GetLogger(this);

            // Store covers in the app data directory under MediaCover
            var appData = global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.ApplicationData);
            _coverRootFolder = global::System.IO.Path.Combine(appData, "Kagarr", "MediaCover");

            if (!global::System.IO.Directory.Exists(_coverRootFolder))
            {
                global::System.IO.Directory.CreateDirectory(_coverRootFolder);
            }
        }

        public void ConvertToLocalUrls(int gameId, global::System.Collections.Generic.IEnumerable<Games.MediaCover> covers)
        {
            if (covers == null)
            {
                return;
            }

            foreach (var cover in covers)
            {
                if (gameId == 0)
                {
                    // Game is not in the library yet; keep the remote URL as-is
                    // so the frontend can display it directly from IGDB CDN
                    continue;
                }

                var localPath = GetCoverPath(gameId, cover.CoverType);
                var localFileName = global::System.IO.Path.GetFileName(localPath);

                // Set the URL to a local API path that the frontend can use
                cover.Url = $"/api/v1/mediacover/{gameId}/{localFileName}";

                // Ensure the cover is downloaded
                EnsureCoverExists(gameId, cover);
            }
        }

        public string GetCoverPath(int gameId, MediaCoverTypes coverType)
        {
            var gameCoverPath = GetGameCoverPath(gameId);
            var extension = ".jpg";

            return global::System.IO.Path.Combine(gameCoverPath, coverType.ToString().ToLowerInvariant() + extension);
        }

        private string GetGameCoverPath(int gameId)
        {
            var path = global::System.IO.Path.Combine(_coverRootFolder, gameId.ToString());

            if (!global::System.IO.Directory.Exists(path))
            {
                global::System.IO.Directory.CreateDirectory(path);
            }

            return path;
        }

        private void EnsureCoverExists(int gameId, Games.MediaCover cover)
        {
            var localPath = GetCoverPath(gameId, cover.CoverType);

            if (global::System.IO.File.Exists(localPath))
            {
                return;
            }

            if (string.IsNullOrEmpty(cover.RemoteUrl))
            {
                return;
            }

            try
            {
                _logger.Debug("Downloading cover for game {0}: {1}", gameId, cover.RemoteUrl);

                using (var httpClient = new HttpClient())
                {
                    var imageBytes = httpClient.GetByteArrayAsync(cover.RemoteUrl).Result;

                    var directory = global::System.IO.Path.GetDirectoryName(localPath);
                    if (!string.IsNullOrEmpty(directory) && !global::System.IO.Directory.Exists(directory))
                    {
                        global::System.IO.Directory.CreateDirectory(directory);
                    }

                    global::System.IO.File.WriteAllBytes(localPath, imageBytes);

                    _logger.Debug("Successfully downloaded cover to {0}", localPath);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Warn(ex, "Failed to download cover for game {0} from {1}", gameId, cover.RemoteUrl);
            }
        }
    }
}
