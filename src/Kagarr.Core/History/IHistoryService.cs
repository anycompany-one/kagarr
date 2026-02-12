using System.Collections.Generic;

namespace Kagarr.Core.History
{
    public interface IHistoryService
    {
        void RecordEvent(string eventType, int gameId, string gameTitle, string sourceTitle, string data = null);
        List<HistoryRecord> GetRecent(int count = 50);
        List<HistoryRecord> GetByGameId(int gameId);
    }
}
