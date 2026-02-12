using System;
using Kagarr.Core.History;
using Kagarr.Http.REST;

namespace Kagarr.Api.V1.History
{
    public class HistoryResource : RestResource
    {
        public string EventType { get; set; }
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public string SourceTitle { get; set; }
        public DateTime Date { get; set; }
        public string Data { get; set; }

        public static HistoryResource FromModel(HistoryRecord record)
        {
            if (record == null)
            {
                return null;
            }

            return new HistoryResource
            {
                Id = record.Id,
                EventType = record.EventType,
                GameId = record.GameId,
                GameTitle = record.GameTitle,
                SourceTitle = record.SourceTitle,
                Date = record.Date,
                Data = record.Data
            };
        }
    }
}
