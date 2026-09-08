using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Indexers.Newznab;
using Kagarr.Core.Indexers.Torznab;
using NLog;

namespace Kagarr.Core.Indexers
{
    public class IndexerService : IIndexerService
    {
        private readonly IIndexerRepository _indexerRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Logger _logger;

        public IndexerService(IIndexerRepository indexerRepository, IHttpClientFactory httpClientFactory)
        {
            _indexerRepository = indexerRepository;
            _httpClientFactory = httpClientFactory;
            _logger = KagarrLogger.GetLogger(this);
        }

        public List<IndexerDefinition> All()
        {
            return _indexerRepository.All().ToList();
        }

        public IndexerDefinition Get(int id)
        {
            return _indexerRepository.Get(id);
        }

        public IndexerDefinition Add(IndexerDefinition indexer)
        {
            return _indexerRepository.Insert(indexer);
        }

        public IndexerDefinition Update(IndexerDefinition indexer)
        {
            return _indexerRepository.Update(indexer);
        }

        public void Delete(int id)
        {
            _indexerRepository.Delete(id);
        }

        public async Task<List<ReleaseInfo>> SearchAllIndexersAsync(string searchTerm)
        {
            var indexerDefinitions = _indexerRepository.All()
                .Where(d => d.EnableSearch)
                .ToList();

            _logger.Info("Searching {0} indexers for '{1}'", indexerDefinitions.Count, searchTerm);

            // Fan out to all indexers in parallel; one failing indexer must not kill the search
            var searchTasks = indexerDefinitions
                .Select(definition => SearchIndexerSafeAsync(definition, searchTerm))
                .ToList();

            var resultsPerIndexer = await Task.WhenAll(searchTasks);

            var allReleases = resultsPerIndexer.SelectMany(r => r).ToList();

            // Sort by seeders (descending) for torrents, then by publish date
            return allReleases
                .OrderByDescending(r => r.Seeders)
                .ThenByDescending(r => r.PublishDate)
                .ToList();
        }

        private async Task<List<ReleaseInfo>> SearchIndexerSafeAsync(IndexerDefinition definition, string searchTerm)
        {
            try
            {
                var indexer = CreateIndexer(definition);
                if (indexer == null)
                {
                    _logger.Warn("Unknown indexer implementation: {0}", definition.Implementation);
                    return new List<ReleaseInfo>();
                }

                var releases = await indexer.SearchAsync(searchTerm);
                releases.ForEach(r => r.IndexerId = definition.Id);
                return releases;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching indexer '{0}'", definition.Name);
                return new List<ReleaseInfo>();
            }
        }

        protected internal virtual IIndexer CreateIndexer(IndexerDefinition definition)
        {
            switch (definition.Implementation?.ToLowerInvariant())
            {
                case "newznab":
                    return NewznabIndexer.FromDefinition(definition, CreateHttpClient());
                case "torznab":
                    return TorznabIndexer.FromDefinition(definition, CreateHttpClient());
                default:
                    return null;
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient("indexer");
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            return httpClient;
        }
    }
}
