using System.Collections.Generic;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.History
{
    public interface IHistoryRepository : IBasicRepository<HistoryRecord>
    {
        List<HistoryRecord> GetRecent(int count);
        List<HistoryRecord> FindByGameId(int gameId);
    }
}
