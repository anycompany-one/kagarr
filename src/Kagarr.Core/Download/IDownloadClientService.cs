using System.Collections.Generic;
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
        string SendToDownloadClient(ReleaseInfo release);
        List<DownloadClientItem> GetQueue();
    }
}
