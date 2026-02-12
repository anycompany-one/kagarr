using System.Linq;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.Download
{
    public class DownloadTrackingRepository : BasicRepository<DownloadTracking>, IDownloadTrackingRepository
    {
        public DownloadTrackingRepository(IDatabase database)
            : base(database)
        {
        }

        public DownloadTracking FindByDownloadId(string downloadId)
        {
            return Query(t => t.DownloadId == downloadId).SingleOrDefault();
        }
    }
}
