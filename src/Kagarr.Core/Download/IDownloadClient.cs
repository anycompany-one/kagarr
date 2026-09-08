using System.Collections.Generic;
using System.Threading.Tasks;
using Kagarr.Core.Indexers;

namespace Kagarr.Core.Download
{
    public interface IDownloadClient
    {
        string Name { get; }
        string Protocol { get; }
        Task<string> DownloadAsync(ReleaseInfo release);
        Task<List<DownloadClientItem>> GetItemsAsync();
    }
}
