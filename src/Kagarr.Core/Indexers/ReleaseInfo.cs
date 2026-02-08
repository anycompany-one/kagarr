using System;
using System.Collections.Generic;

namespace Kagarr.Core.Indexers
{
    public class ReleaseInfo
    {
        public string Guid { get; set; }
        public string Title { get; set; }
        public long Size { get; set; }
        public string DownloadUrl { get; set; }
        public string InfoUrl { get; set; }
        public string Indexer { get; set; }
        public int IndexerId { get; set; }
        public string DownloadProtocol { get; set; }
        public DateTime PublishDate { get; set; }
        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public List<string> Categories { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"[{Indexer}] {Title} ({FormatSize(Size)})";
        }

        private static string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            var order = 0;
            var size = (double)bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}
