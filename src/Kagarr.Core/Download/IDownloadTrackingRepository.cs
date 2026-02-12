using Kagarr.Core.Datastore;

namespace Kagarr.Core.Download
{
    public interface IDownloadTrackingRepository : IBasicRepository<DownloadTracking>
    {
        DownloadTracking FindByDownloadId(string downloadId);
    }
}
