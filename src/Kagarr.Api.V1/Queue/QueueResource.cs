using Kagarr.Core.Download;

namespace Kagarr.Api.V1.Queue
{
    public class QueueResource
    {
        public string DownloadId { get; set; }
        public string Title { get; set; }
        public long TotalSize { get; set; }
        public long RemainingSize { get; set; }
        public string OutputPath { get; set; }
        public string Status { get; set; }
        public string DownloadClient { get; set; }

        public static QueueResource FromModel(DownloadClientItem item)
        {
            if (item == null)
            {
                return null;
            }

            return new QueueResource
            {
                DownloadId = item.DownloadId,
                Title = item.Title,
                TotalSize = item.TotalSize,
                RemainingSize = item.RemainingSize,
                OutputPath = item.OutputPath,
                Status = item.Status.ToString(),
                DownloadClient = item.DownloadClientName
            };
        }
    }
}
