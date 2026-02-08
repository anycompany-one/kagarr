using System.Collections.Generic;
using Kagarr.Core.Indexers;

namespace Kagarr.Core.Download
{
    public interface IDownloadClient
    {
        string Name { get; }
        string Protocol { get; }
        string Download(ReleaseInfo release);
        List<DownloadClientItem> GetItems();
    }
}
