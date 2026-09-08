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
            // FirstOrDefault: duplicate DownloadIds must not crash the import poll
            return QueryWhere("\"DownloadId\" = @DownloadId", new { DownloadId = downloadId }).FirstOrDefault();
        }
    }
}
