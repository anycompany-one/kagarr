using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Download.Clients.QBittorrent;
using Kagarr.Core.Download.Clients.Sabnzbd;
using Kagarr.Core.History;
using Kagarr.Core.Http;
using Kagarr.Core.Indexers;
using Newtonsoft.Json.Linq;
using NLog;

namespace Kagarr.Core.Download
{
    public class DownloadClientService : IDownloadClientService
    {
        private readonly IDownloadClientRepository _downloadClientRepository;
        private readonly IDownloadTrackingRepository _trackingRepository;
        private readonly IHistoryService _historyService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Logger _logger;

        public DownloadClientService(
            IDownloadClientRepository downloadClientRepository,
            IDownloadTrackingRepository trackingRepository,
            IHistoryService historyService,
            IHttpClientFactory httpClientFactory)
        {
            _downloadClientRepository = downloadClientRepository;
            _trackingRepository = trackingRepository;
            _historyService = historyService;
            _httpClientFactory = httpClientFactory;
            _logger = KagarrLogger.GetLogger(this);
        }

        public List<DownloadClientDefinition> All()
        {
            return _downloadClientRepository.All().ToList();
        }

        public DownloadClientDefinition Get(int id)
        {
            return _downloadClientRepository.Get(id);
        }

        public DownloadClientDefinition Add(DownloadClientDefinition client)
        {
            return _downloadClientRepository.Insert(client);
        }

        public DownloadClientDefinition Update(DownloadClientDefinition client)
        {
            return _downloadClientRepository.Update(client);
        }

        public void Delete(int id)
        {
            _downloadClientRepository.Delete(id);
        }

        public async Task<string> SendToDownloadClientAsync(ReleaseInfo release, int gameId = 0, string gameTitle = null)
        {
            var definitions = _downloadClientRepository.All()
                .Where(d => d.Enable && d.Protocol == release.DownloadProtocol)
                .OrderBy(d => d.Priority)
                .ToList();

            if (definitions.Count == 0)
            {
                throw new InvalidOperationException($"No download client configured for protocol '{release.DownloadProtocol}'");
            }

            foreach (var definition in definitions)
            {
                try
                {
                    var client = CreateClient(definition);
                    if (client == null)
                    {
                        _logger.Warn("Unknown download client implementation: {0}", definition.Implementation);
                        continue;
                    }

                    var downloadId = await client.DownloadAsync(release);

                    // Track the download so CompletedDownloadJob can auto-import it
                    if (gameId > 0 && !string.IsNullOrWhiteSpace(downloadId))
                    {
                        var existing = _trackingRepository.FindByDownloadId(downloadId);
                        if (existing != null)
                        {
                            _logger.Info("Download '{0}' is already tracked, not creating a duplicate tracking record", downloadId);
                        }
                        else
                        {
                            _trackingRepository.Insert(new DownloadTracking
                            {
                                DownloadId = downloadId,
                                GameId = gameId,
                                GameTitle = gameTitle ?? release.Title,
                                SourceTitle = release.Title,
                                AddedDate = DateTime.UtcNow
                            });
                        }

                        _historyService.RecordEvent(
                            HistoryEventType.Grabbed,
                            gameId,
                            gameTitle ?? release.Title,
                            release.Title,
                            $"Sent to download client via {release.Indexer}");
                    }

                    return downloadId;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to send release to download client '{0}'", definition.Name);

                    // If this is the last client, throw
                    if (definition == definitions.Last())
                    {
                        throw;
                    }
                }
            }

            throw new InvalidOperationException("All download clients failed");
        }

        public async Task<List<DownloadClientItem>> GetQueueAsync()
        {
            var allItems = new List<DownloadClientItem>();
            var definitions = _downloadClientRepository.All()
                .Where(d => d.Enable)
                .ToList();

            foreach (var definition in definitions)
            {
                try
                {
                    var client = CreateClient(definition);
                    if (client == null)
                    {
                        continue;
                    }

                    var items = await client.GetItemsAsync();
                    var clientHost = GetHostFromSettings(definition.Settings);
                    foreach (var item in items)
                    {
                        item.DownloadProtocol = definition.Protocol;
                        item.DownloadClientHost = clientHost;
                    }

                    allItems.AddRange(items);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to get items from download client '{0}'", definition.Name);
                }
            }

            return allItems;
        }

        protected internal virtual IDownloadClient CreateClient(DownloadClientDefinition definition)
        {
            switch (definition.Implementation?.ToLowerInvariant())
            {
                case "qbittorrent":
                    return QBittorrentClient.FromDefinition(definition, _httpClientFactory.CreateClient(HttpClientNames.NoCookies));
                case "sabnzbd":
                    return SabnzbdClient.FromDefinition(definition, CreateDefaultClient());
                default:
                    return null;
            }
        }

        private HttpClient CreateDefaultClient()
        {
            var httpClient = _httpClientFactory.CreateClient("downloadclient");
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            return httpClient;
        }

        private static string GetHostFromSettings(string settingsJson)
        {
            if (string.IsNullOrWhiteSpace(settingsJson))
            {
                return null;
            }

            try
            {
                var json = JObject.Parse(settingsJson);
                return json["host"]?.Value<string>();
            }
            catch
            {
                return null;
            }
        }
    }
}
