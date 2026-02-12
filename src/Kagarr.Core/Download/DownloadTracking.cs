using System;
using Kagarr.Core.Datastore;

namespace Kagarr.Core.Download
{
    public class DownloadTracking : ModelBase
    {
        public string DownloadId { get; set; }
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public string SourceTitle { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
