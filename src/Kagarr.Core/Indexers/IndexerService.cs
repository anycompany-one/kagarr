using System;
using System.Collections.Generic;
using System.Linq;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Indexers.Newznab;
using Kagarr.Core.Indexers.Torznab;
using NLog;

namespace Kagarr.Core.Indexers
{
    public class IndexerService : IIndexerService
    {
        private readonly IIndexerRepository _indexerRepository;
        private readonly Logger _logger;

        public IndexerService(IIndexerRepository indexerRepository)
        {
            _indexerRepository = indexerRepository;
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

        public List<ReleaseInfo> SearchAllIndexers(string searchTerm)
        {
            var allReleases = new List<ReleaseInfo>();
            var indexerDefinitions = _indexerRepository.All()
                .Where(d => d.EnableSearch)
                .ToList();

            _logger.Info("Searching {0} indexers for '{1}'", indexerDefinitions.Count, searchTerm);

            foreach (var definition in indexerDefinitions)
            {
                try
                {
                    var indexer = CreateIndexer(definition);
                    if (indexer == null)
                    {
                        _logger.Warn("Unknown indexer implementation: {0}", definition.Implementation);
                        continue;
                    }

                    indexer.Search(searchTerm).ForEach(r =>
                    {
                        r.IndexerId = definition.Id;
                        allReleases.Add(r);
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error searching indexer '{0}'", definition.Name);
                }
            }

            // Sort by seeders (descending) for torrents, then by publish date
            return allReleases
                .OrderByDescending(r => r.Seeders)
                .ThenByDescending(r => r.PublishDate)
                .ToList();
        }

        private static IIndexer CreateIndexer(IndexerDefinition definition)
        {
            switch (definition.Implementation?.ToLowerInvariant())
            {
                case "newznab":
                    return NewznabIndexer.FromDefinition(definition);
                case "torznab":
                    return TorznabIndexer.FromDefinition(definition);
                default:
                    return null;
            }
        }
    }
}
