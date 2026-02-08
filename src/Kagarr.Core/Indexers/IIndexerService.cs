using System.Collections.Generic;

namespace Kagarr.Core.Indexers
{
    public interface IIndexerService
    {
        List<IndexerDefinition> All();
        IndexerDefinition Get(int id);
        IndexerDefinition Add(IndexerDefinition indexer);
        IndexerDefinition Update(IndexerDefinition indexer);
        void Delete(int id);
        List<ReleaseInfo> SearchAllIndexers(string searchTerm);
    }
}
