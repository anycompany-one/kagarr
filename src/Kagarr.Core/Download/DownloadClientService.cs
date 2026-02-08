using System;
using System.Collections.Generic;
using System.Linq;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Download.Clients.QBittorrent;
using Kagarr.Core.Download.Clients.Sabnzbd;
using Kagarr.Core.Indexers;
using NLog;

namespace Kagarr.Core.Download
{
    public class DownloadClientService : IDownloadClientService
    {
        private readonly IDownloadClientRepository _downloadClientRepository;
        private readonly Logger _logger;

        public DownloadClientService(IDownloadClientRepository downloadClientRepository)
        {
            _downloadClientRepository = downloadClientRepository;
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

        public string SendToDownloadClient(ReleaseInfo release)
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

                    return client.Download(release);
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

        public List<DownloadClientItem> GetQueue()
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

                    allItems.AddRange(client.GetItems());
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to get items from download client '{0}'", definition.Name);
                }
            }

            return allItems;
        }

        private static IDownloadClient CreateClient(DownloadClientDefinition definition)
        {
            switch (definition.Implementation?.ToLowerInvariant())
            {
                case "qbittorrent":
                    return QBittorrentClient.FromDefinition(definition);
                case "sabnzbd":
                    return SabnzbdClient.FromDefinition(definition);
                default:
                    return null;
            }
        }
    }
}
