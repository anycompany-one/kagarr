using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Indexers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Kagarr.Core.Download.Clients.QBittorrent
{
    public class QBittorrentClient : IDownloadClient
    {
        private readonly Logger _logger;
        private readonly QBittorrentSettings _settings;
        private readonly string _name;
        private readonly CookieContainer _cookies;

        public QBittorrentClient(string name, QBittorrentSettings settings)
        {
            _name = name;
            _settings = settings;
            _logger = KagarrLogger.GetLogger(this);
            _cookies = new CookieContainer();
        }

        public string Name => _name;
        public string Protocol => "torrent";

        public string Download(ReleaseInfo release)
        {
            _logger.Info("Sending '{0}' to qBittorrent", release.Title);

            Authenticate();

            var baseUrl = _settings.GetBaseUrl();

            using (var handler = new HttpClientHandler { CookieContainer = _cookies })
            using (var httpClient = new HttpClient(handler))
            {
                var infoHash = TryGetInfoHash(release.DownloadUrl);

                // If the hash cannot be determined from the URL, snapshot the existing
                // torrents so the newly added one can be identified afterwards.
                HashSet<string> existingHashes = null;
                if (infoHash == null)
                {
                    existingHashes = GetTorrentHashes(httpClient, baseUrl);
                }

                var content = new MultipartFormDataContent
                {
                    { new StringContent(release.DownloadUrl), "urls" },
                    { new StringContent(_settings.Category), "category" }
                };

                var response = httpClient.PostAsync($"{baseUrl}/api/v2/torrents/add", content).Result;

                if (!response.IsSuccessStatusCode)
                {
                    var body = response.Content.ReadAsStringAsync().Result;
                    _logger.Error("Failed to add torrent to qBittorrent. Status: {0}, Body: {1}", response.StatusCode, body);
                    throw new HttpRequestException($"qBittorrent API error: {response.StatusCode}");
                }

                if (infoHash == null)
                {
                    infoHash = PollForNewTorrentHash(httpClient, baseUrl, existingHashes);
                }

                if (infoHash == null)
                {
                    _logger.Warn("Could not determine info hash for '{0}', tracking may not match", release.Title);
                    infoHash = release.Guid ?? release.Title;
                }

                _logger.Info("Successfully sent '{0}' to qBittorrent (hash: {1})", release.Title, infoHash);
                return infoHash;
            }
        }

        public static string TryGetInfoHash(string downloadLink)
        {
            if (string.IsNullOrWhiteSpace(downloadLink))
            {
                return null;
            }

            string candidate = null;

            if (downloadLink.StartsWith("magnet:", StringComparison.OrdinalIgnoreCase))
            {
                var match = global::System.Text.RegularExpressions.Regex.Match(
                    downloadLink, @"xt=urn:btih:([A-Za-z0-9]+)", global::System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    candidate = match.Groups[1].Value;
                }
            }
            else
            {
                var match = global::System.Text.RegularExpressions.Regex.Match(downloadLink, @"\b([A-Fa-f0-9]{40})\b");
                if (match.Success)
                {
                    candidate = match.Groups[1].Value;
                }
            }

            if (candidate == null)
            {
                return null;
            }

            // Hex form (40 chars)
            if (candidate.Length == 40 && global::System.Text.RegularExpressions.Regex.IsMatch(candidate, "^[A-Fa-f0-9]{40}$"))
            {
                return candidate.ToLowerInvariant();
            }

            // Base32 form (32 chars) - convert to lowercase hex
            if (candidate.Length == 32)
            {
                var bytes = TryDecodeBase32(candidate.ToUpperInvariant());
                if (bytes != null)
                {
                    return Convert.ToHexString(bytes).ToLowerInvariant();
                }
            }

            return null;
        }

        private static byte[] TryDecodeBase32(string input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var bits = 0;
            var bitCount = 0;
            var output = new List<byte>(input.Length * 5 / 8);

            foreach (var c in input)
            {
                var index = alphabet.IndexOf(c);
                if (index < 0)
                {
                    return null;
                }

                bits = (bits << 5) | index;
                bitCount += 5;

                if (bitCount >= 8)
                {
                    bitCount -= 8;
                    output.Add((byte)((bits >> bitCount) & 0xFF));
                }
            }

            return output.ToArray();
        }

        private HashSet<string> GetTorrentHashes(HttpClient httpClient, string baseUrl)
        {
            var url = $"{baseUrl}/api/v2/torrents/info?category={Uri.EscapeDataString(_settings.Category)}";
            var response = httpClient.GetStringAsync(url).Result;
            var torrents = JsonConvert.DeserializeObject<List<JObject>>(response) ?? new List<JObject>();

            return torrents
                .Select(t => t["hash"]?.ToString()?.ToLowerInvariant())
                .Where(h => !string.IsNullOrEmpty(h))
                .ToHashSet();
        }

        private string PollForNewTorrentHash(HttpClient httpClient, string baseUrl, HashSet<string> existingHashes)
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                global::System.Threading.Thread.Sleep(500);

                try
                {
                    var newHash = GetTorrentHashes(httpClient, baseUrl)
                        .FirstOrDefault(h => !existingHashes.Contains(h));
                    if (newHash != null)
                    {
                        return newHash;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to poll qBittorrent for new torrent hash");
                }
            }

            return null;
        }

        public List<DownloadClientItem> GetItems()
        {
            Authenticate();

            var baseUrl = _settings.GetBaseUrl();

            using (var handler = new HttpClientHandler { CookieContainer = _cookies })
            using (var httpClient = new HttpClient(handler))
            {
                var url = $"{baseUrl}/api/v2/torrents/info?category={Uri.EscapeDataString(_settings.Category)}";
                var response = httpClient.GetStringAsync(url).Result;
                var torrents = JsonConvert.DeserializeObject<List<JObject>>(response) ?? new List<JObject>();

                return torrents.Select(t => new DownloadClientItem
                {
                    DownloadId = t["hash"]?.ToString()?.ToLowerInvariant(),
                    Title = t["name"]?.ToString(),
                    TotalSize = t["total_size"]?.Value<long>() ?? 0,
                    RemainingSize = (t["total_size"]?.Value<long>() ?? 0) - (t["completed"]?.Value<long>() ?? 0),
                    OutputPath = t["content_path"]?.ToString(),
                    Category = t["category"]?.ToString(),
                    Status = MapStatus(t["state"]?.ToString()),
                    DownloadClientName = _name
                }).ToList();
            }
        }

        private void Authenticate()
        {
            if (string.IsNullOrEmpty(_settings.Username))
            {
                return;
            }

            var baseUrl = _settings.GetBaseUrl();

            using (var handler = new HttpClientHandler { CookieContainer = _cookies })
            using (var httpClient = new HttpClient(handler))
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", _settings.Username),
                    new KeyValuePair<string, string>("password", _settings.Password ?? string.Empty)
                });

                var response = httpClient.PostAsync($"{baseUrl}/api/v2/auth/login", content).Result;
                var body = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode || body.Contains("Fails", StringComparison.OrdinalIgnoreCase))
                {
                    throw new HttpRequestException("Failed to authenticate with qBittorrent");
                }
            }
        }

        private static DownloadItemStatus MapStatus(string state)
        {
            switch (state)
            {
                case "uploading":
                case "stalledUP":
                case "pausedUP":
                case "queuedUP":
                case "forcedUP":
                    return DownloadItemStatus.Completed;

                case "downloading":
                case "stalledDL":
                case "forcedDL":
                case "metaDL":
                    return DownloadItemStatus.Downloading;

                case "pausedDL":
                case "queuedDL":
                    return DownloadItemStatus.Paused;

                case "error":
                case "missingFiles":
                    return DownloadItemStatus.Failed;

                default:
                    return DownloadItemStatus.Queued;
            }
        }

        public static QBittorrentClient FromDefinition(DownloadClientDefinition definition)
        {
            var settings = JsonConvert.DeserializeObject<QBittorrentSettings>(definition.Settings) ?? new QBittorrentSettings();
            return new QBittorrentClient(definition.Name, settings);
        }
    }
}
