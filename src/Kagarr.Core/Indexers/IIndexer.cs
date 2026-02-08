using System.Collections.Generic;

namespace Kagarr.Core.Indexers
{
    public interface IIndexer
    {
        string Name { get; }
        string Protocol { get; }
        List<ReleaseInfo> Search(string searchTerm);
    }
}
