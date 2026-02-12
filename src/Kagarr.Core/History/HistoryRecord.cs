using System;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.History
{
    public class HistoryRecord : ModelBase
    {
        public string EventType { get; set; }
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public string SourceTitle { get; set; }
        public DateTime Date { get; set; }
        public string Data { get; set; }
    }

    public static class HistoryEventType
    {
        public const string Grabbed = "Grabbed";
        public const string Imported = "Imported";
        public const string ImportFailed = "ImportFailed";
        public const string Deleted = "Deleted";
    }
}
