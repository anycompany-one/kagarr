namespace Kagarr.Core.Download
{
    public class DownloadClientItem
    {
        public string DownloadId { get; set; }
        public string Title { get; set; }
        public long TotalSize { get; set; }
        public long RemainingSize { get; set; }
        public string OutputPath { get; set; }
        public string Category { get; set; }
        public DownloadItemStatus Status { get; set; }
        public string Message { get; set; }
        public string DownloadClientName { get; set; }
        public string DownloadProtocol { get; set; }
    }

    public enum DownloadItemStatus
    {
        Queued = 0,
        Downloading = 1,
        Paused = 2,
        Completed = 3,
        Failed = 4
    }
}
