using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Games;
using Newtonsoft.Json;
using NLog;

namespace Kagarr.Core.MetadataSource.Igdb
{
    public class IgdbProxy : ISearchForNewGame
    {
        private const string IgdbApiBaseUrl = "https://api.igdb.com/v4";
        private const string IgdbImageBaseUrl = "https://images.igdb.com/igdb/image/upload";

        private readonly IIgdbAuthService _authService;
        private readonly Logger _logger;

        public IgdbProxy(IIgdbAuthService authService)
        {
            _authService = authService;
            _logger = KagarrLogger.GetLogger(this);
        }

        public List<Game> SearchForNewGame(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return new List<Game>();
            }

            _logger.Info("Searching IGDB for '{0}'", term);

            // Escape double quotes in search term
            var escapedTerm = term.Replace("\"", "\\\"");

            var body = $"search \"{escapedTerm}\"; fields name,cover.*,summary,platforms.*,first_release_date,genres.*,involved_companies.*,involved_companies.company.*,screenshots.*; limit 20;";

            var resources = ExecuteIgdbQuery<IgdbGameResource>("games", body);

            return resources.Select(MapToGame).ToList();
        }

        public Game GetGameInfo(int igdbId)
        {
            _logger.Info("Getting game info from IGDB for ID {0}", igdbId);

            var body = $"where id = {igdbId}; fields name,cover.*,summary,platforms.*,first_release_date,genres.*,involved_companies.*,involved_companies.company.*,screenshots.*; limit 1;";

            var resources = ExecuteIgdbQuery<IgdbGameResource>("games", body);
            var resource = resources.FirstOrDefault();

            if (resource == null)
            {
                _logger.Warn("No game found on IGDB with ID {0}", igdbId);
                return null;
            }

            return MapToGame(resource);
        }

        private List<T> ExecuteIgdbQuery<T>(string endpoint, string body)
        {
            var token = _authService.GetAccessToken();
            var clientId = _authService.GetClientId();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                using (var content = new StringContent(body, Encoding.UTF8, "text/plain"))
                {
                    var url = $"{IgdbApiBaseUrl}/{endpoint}";

                    _logger.Debug("IGDB API request: POST {0} - {1}", url, body);

                    var response = httpClient.PostAsync(url, content).Result;
                    var responseBody = response.Content.ReadAsStringAsync().Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.Error("IGDB API error. Status: {0}, Body: {1}", response.StatusCode, responseBody);
                        throw new HttpRequestException($"IGDB API request failed with status {response.StatusCode}");
                    }

                    return JsonConvert.DeserializeObject<List<T>>(responseBody) ?? new List<T>();
                }
            }
        }

        private static Game MapToGame(IgdbGameResource resource)
        {
            var game = new Game
            {
                IgdbId = resource.Id,
                Title = resource.Name,
                CleanTitle = resource.Name?.ToLowerInvariant().Replace(" ", ""),
                SortTitle = resource.Name,
                Overview = resource.Summary,
                Genres = resource.Genres?.Select(g => g.Name).ToList() ?? new List<string>(),
                Images = new List<Games.MediaCover>()
            };

            // Map release date
            if (resource.FirstReleaseDate.HasValue)
            {
                var epoch = DateTimeOffset.FromUnixTimeSeconds(resource.FirstReleaseDate.Value);
                game.ReleaseDate = epoch.UtcDateTime;
                game.Year = epoch.Year;
            }

            // Map developer and publisher from involved companies
            if (resource.InvolvedCompanies != null)
            {
                var developer = resource.InvolvedCompanies.FirstOrDefault(c => c.Developer);
                if (developer?.Company != null)
                {
                    game.Developer = developer.Company.Name;
                }

                var publisher = resource.InvolvedCompanies.FirstOrDefault(c => c.Publisher);
                if (publisher?.Company != null)
                {
                    game.Publisher = publisher.Company.Name;
                }
            }

            // Map platform - pick the first recognized one, default to PC
            game.Platform = MapPlatform(resource.Platforms);

            // Map cover image
            if (resource.Cover != null && !string.IsNullOrEmpty(resource.Cover.ImageId))
            {
                game.Images.Add(new Games.MediaCover
                {
                    CoverType = MediaCoverTypes.Cover,
                    RemoteUrl = GetImageUrl(resource.Cover.ImageId, "cover_big"),
                    Url = GetImageUrl(resource.Cover.ImageId, "cover_big")
                });
            }

            // Map screenshots
            if (resource.Screenshots != null)
            {
                foreach (var screenshot in resource.Screenshots.Take(5))
                {
                    if (!string.IsNullOrEmpty(screenshot.ImageId))
                    {
                        game.Images.Add(new Games.MediaCover
                        {
                            CoverType = MediaCoverTypes.Screenshot,
                            RemoteUrl = GetImageUrl(screenshot.ImageId, "screenshot_big"),
                            Url = GetImageUrl(screenshot.ImageId, "screenshot_big")
                        });
                    }
                }
            }

            return game;
        }

        private static string GetImageUrl(string imageId, string size)
        {
            return $"{IgdbImageBaseUrl}/t_{size}/{imageId}.jpg";
        }

        private static Platforms.GamePlatform MapPlatform(List<IgdbPlatformResource> platforms)
        {
            if (platforms == null || platforms.Count == 0)
            {
                return Platforms.GamePlatform.Unknown;
            }

            // IGDB platform IDs: https://api-docs.igdb.com/#platform
            // 6 = PC (Windows), 14 = Mac, 3 = Linux
            // 48 = PS4, 167 = PS5
            // 49 = Xbox One, 169 = Xbox Series X|S
            // 130 = Nintendo Switch
            foreach (var platform in platforms)
            {
                switch (platform.Id)
                {
                    case 6:
                    case 14:
                    case 3:
                        return Platforms.GamePlatform.PC;
                    case 167:
                        return Platforms.GamePlatform.PlayStation5;
                    case 48:
                        return Platforms.GamePlatform.PlayStation4;
                    case 169:
                        return Platforms.GamePlatform.Xbox_Series;
                    case 49:
                        return Platforms.GamePlatform.Xbox_One;
                    case 130:
                        return Platforms.GamePlatform.NintendoSwitch;
                    case 18:
                        return Platforms.GamePlatform.NES;
                    case 19:
                        return Platforms.GamePlatform.SNES;
                    case 4:
                        return Platforms.GamePlatform.N64;
                    case 33:
                        return Platforms.GamePlatform.GameBoy;
                    case 24:
                        return Platforms.GamePlatform.GBA;
                    case 20:
                        return Platforms.GamePlatform.DS;
                    case 29:
                        return Platforms.GamePlatform.Genesis;
                    case 23:
                        return Platforms.GamePlatform.Dreamcast;
                    case 7:
                        return Platforms.GamePlatform.PS1;
                    case 8:
                        return Platforms.GamePlatform.PS2;
                    case 38:
                        return Platforms.GamePlatform.PSP;
                    default:
                        continue;
                }
            }

            // Default to PC if no known platform matched
            return Platforms.GamePlatform.PC;
        }
    }
}
