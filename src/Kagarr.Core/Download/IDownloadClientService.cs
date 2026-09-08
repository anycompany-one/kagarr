using System.Collections.Generic;
using System.Threading.Tasks;
using Kagarr.Core.Indexers;

namespace Kagarr.Core.Download
{
    public interface IDownloadClientService
    {
        List<DownloadClientDefinition> All();
        DownloadClientDefinition Get(int id);
        DownloadClientDefinition Add(DownloadClientDefinition client);
        DownloadClientDefinition Update(DownloadClientDefinition client);
        void Delete(int id);
        Task<string> SendToDownloadClientAsync(ReleaseInfo release, int gameId = 0, string gameTitle = null);
        Task<List<DownloadClientItem>> GetQueueAsync();
    }
}
