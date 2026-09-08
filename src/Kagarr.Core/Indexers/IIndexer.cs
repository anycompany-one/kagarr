using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kagarr.Core.Indexers
{
    public interface IIndexer
    {
        string Name { get; }
        string Protocol { get; }
        Task<List<ReleaseInfo>> SearchAsync(string searchTerm);
    }
}
