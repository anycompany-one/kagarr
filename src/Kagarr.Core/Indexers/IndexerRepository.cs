using Kagarr.Core.Datastore;

namespace Kagarr.Core.Indexers
{
    public class IndexerRepository : BasicRepository<IndexerDefinition>, IIndexerRepository
    {
        public IndexerRepository(IDatabase database)
            : base(database)
        {
        }
    }
}
