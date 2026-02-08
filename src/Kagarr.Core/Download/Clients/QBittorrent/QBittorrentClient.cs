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

                _logger.Info("Successfully sent '{0}' to qBittorrent", release.Title);
                return release.Guid ?? release.Title;
            }
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
                    DownloadId = t["hash"]?.ToString(),
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
