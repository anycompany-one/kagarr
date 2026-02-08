using System;
using System.Collections.Generic;
using Kagarr.Core.Datastore;
using Kagarr.Core.Platforms;

namespace Kagarr.Core.Games
{
    public class Game : ModelBase
    {
        public Game()
        {
            Genres = new List<string>();
            Tags = new HashSet<int>();
            Images = new List<MediaCover>();
        }

        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public int Year { get; set; }
        public string Overview { get; set; }
        public int IgdbId { get; set; }
        public int? SteamAppId { get; set; }
        public GamePlatform Platform { get; set; }
        public List<string> Genres { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<MediaCover> Images { get; set; }
        public string Path { get; set; }
        public int? GameFileId { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public string RootFolderPath { get; set; }

        public override string ToString()
        {
            return $"[{IgdbId}][{Title ?? "Unknown"}]";
        }
    }

    public class MediaCover
    {
        public MediaCoverTypes CoverType { get; set; }
        public string Url { get; set; }
        public string RemoteUrl { get; set; }
    }

    public enum MediaCoverTypes
    {
        Unknown = 0,
        Cover = 1,
        Screenshot = 2,
        Banner = 3,
        Fanart = 4
    }
}
