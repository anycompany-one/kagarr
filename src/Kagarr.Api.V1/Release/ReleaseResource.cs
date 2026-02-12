using System;
using System.Collections.Generic;
using Kagarr.Core.Indexers;

namespace Kagarr.Api.V1.Release
{
    public class ReleaseResource
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
        public List<string> Categories { get; set; }

        // Populated by the frontend when grabbing from a game detail page
        public int? GameId { get; set; }
        public string GameTitle { get; set; }

        public static ReleaseResource FromModel(ReleaseInfo model)
        {
            if (model == null)
            {
                return null;
            }

            return new ReleaseResource
            {
                Guid = model.Guid,
                Title = model.Title,
                Size = model.Size,
                DownloadUrl = model.DownloadUrl,
                InfoUrl = model.InfoUrl,
                Indexer = model.Indexer,
                IndexerId = model.IndexerId,
                DownloadProtocol = model.DownloadProtocol,
                PublishDate = model.PublishDate,
                Seeders = model.Seeders,
                Leechers = model.Leechers,
                Categories = model.Categories
            };
        }

        public ReleaseInfo ToModel()
        {
            return new ReleaseInfo
            {
                Guid = Guid,
                Title = Title,
                Size = Size,
                DownloadUrl = DownloadUrl,
                InfoUrl = InfoUrl,
                Indexer = Indexer,
                IndexerId = IndexerId,
                DownloadProtocol = DownloadProtocol,
                PublishDate = PublishDate,
                Seeders = Seeders,
                Leechers = Leechers,
                Categories = Categories ?? new List<string>()
            };
        }
    }
}
