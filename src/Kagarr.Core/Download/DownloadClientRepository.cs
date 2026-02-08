using Kagarr.Core.Datastore;

namespace Kagarr.Core.Download
{
    public class DownloadClientRepository : BasicRepository<DownloadClientDefinition>, IDownloadClientRepository
    {
        public DownloadClientRepository(IDatabase database)
            : base(database)
        {
        }
    }
}
