using System;
using System.Collections.Generic;
using Kagarr.Common.Instrumentation;
using NLog;

namespace Kagarr.Core.History
{
    public class HistoryService : IHistoryService
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public HistoryService(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
            _logger = KagarrLogger.GetLogger(this);
        }

        public void RecordEvent(string eventType, int gameId, string gameTitle, string sourceTitle, string data = null)
        {
            _logger.Info("Recording history event: {0} for '{1}'", eventType, gameTitle);

            var record = new HistoryRecord
            {
                EventType = eventType,
                GameId = gameId,
                GameTitle = gameTitle,
                SourceTitle = sourceTitle,
                Date = DateTime.UtcNow,
                Data = data
            };

            _historyRepository.Insert(record);
        }

        public List<HistoryRecord> GetRecent(int count = 50)
        {
            return _historyRepository.GetRecent(count);
        }

        public List<HistoryRecord> GetByGameId(int gameId)
        {
            return _historyRepository.FindByGameId(gameId);
        }
    }
}
