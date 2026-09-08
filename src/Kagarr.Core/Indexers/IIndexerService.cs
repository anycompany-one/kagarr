using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kagarr.Core.Indexers
{
    public interface IIndexerService
    {
        List<IndexerDefinition> All();
        IndexerDefinition Get(int id);
        IndexerDefinition Add(IndexerDefinition indexer);
        IndexerDefinition Update(IndexerDefinition indexer);
        void Delete(int id);
        Task<List<ReleaseInfo>> SearchAllIndexersAsync(string searchTerm);
    }
}
