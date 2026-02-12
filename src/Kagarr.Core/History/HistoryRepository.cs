using System.Collections.Generic;
using System.Linq;
using Dapper;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.History
{
    public class HistoryRepository : BasicRepository<HistoryRecord>, IHistoryRepository
    {
        public HistoryRepository(IDatabase database)
            : base(database)
        {
        }

        public List<HistoryRecord> GetRecent(int count)
        {
            using var conn = _database.OpenConnection();
            return conn.Query<HistoryRecord>(
                $"SELECT * FROM \"{_table}\" ORDER BY \"Date\" DESC LIMIT @Count",
                new { Count = count }).ToList();
        }

        public List<HistoryRecord> FindByGameId(int gameId)
        {
            return Query(h => h.GameId == gameId)
                .OrderByDescending(h => h.Date)
                .ToList();
        }
    }
}
