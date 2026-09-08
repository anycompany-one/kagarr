using System.Net.Http;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Games;
using NLog;

namespace Kagarr.Core.MediaCovers
{
    public class MediaCoverService : IMapCoversToLocal
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Logger _logger;
        private readonly string _coverRootFolder;

        public MediaCoverService(string dataPath, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = KagarrLogger.GetLogger(this);

            // Store covers in the configured data directory so they survive
            // container recreation (dataPath comes from --data/KAGARR_DATA).
            _coverRootFolder = global::System.IO.Path.Combine(dataPath, "MediaCover");

            if (!global::System.IO.Directory.Exists(_coverRootFolder))
            {
                global::System.IO.Directory.CreateDirectory(_coverRootFolder);
            }
        }

        public async Task ConvertToLocalUrlsAsync(int gameId, global::System.Collections.Generic.IEnumerable<Games.MediaCover> covers)
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
                await EnsureCoverExistsAsync(gameId, cover);
            }
        }

        public string GetCoverPath(int gameId, MediaCoverTypes coverType)
        {
            var gameCoverPath = GetGameCoverPath(gameId);
            var extension = ".jpg";

            return global::System.IO.Path.Combine(gameCoverPath, coverType.ToString().ToLowerInvariant() + extension);
        }

        public string GetStoredCoverPath(int gameId, string fileName)
        {
            if (!IsSafeFileName(fileName))
            {
                return null;
            }

            var path = global::System.IO.Path.Combine(_coverRootFolder, gameId.ToString(), fileName);

            return global::System.IO.File.Exists(path) ? path : null;
        }

        public static bool IsSafeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            // Reject anything that could escape the game's cover folder:
            // path separators, parent-directory segments or rooted paths.
            if (fileName.Contains('/') || fileName.Contains('\\') || fileName.Contains(".."))
            {
                return false;
            }

            if (global::System.IO.Path.IsPathRooted(fileName))
            {
                return false;
            }

            return global::System.IO.Path.GetFileName(fileName) == fileName;
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

        private async Task EnsureCoverExistsAsync(int gameId, Games.MediaCover cover)
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

                var httpClient = _httpClientFactory.CreateClient("mediacover");
                var imageBytes = await httpClient.GetByteArrayAsync(cover.RemoteUrl);

                var directory = global::System.IO.Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(directory) && !global::System.IO.Directory.Exists(directory))
                {
                    global::System.IO.Directory.CreateDirectory(directory);
                }

                await global::System.IO.File.WriteAllBytesAsync(localPath, imageBytes);

                _logger.Debug("Successfully downloaded cover to {0}", localPath);
            }
            catch (HttpRequestException ex)
            {
                _logger.Warn(ex, "Failed to download cover for game {0} from {1}", gameId, cover.RemoteUrl);
            }
        }
    }
}
