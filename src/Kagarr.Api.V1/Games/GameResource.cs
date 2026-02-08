using System;
using System.Collections.Generic;
using Kagarr.Core.Games;
using Kagarr.Core.Platforms;
using Kagarr.Http.REST;

namespace Kagarr.Api.V1.Games
{
    public class GameResource : RestResource
    {
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
        public string RemoteCover { get; set; }
        public string Path { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime Added { get; set; }
        public string RootFolderPath { get; set; }

        public static GameResource FromModel(Kagarr.Core.Games.Game game)
        {
            if (game == null)
            {
                return null;
            }

            var resource = new GameResource
            {
                Id = game.Id,
                Title = game.Title,
                CleanTitle = game.CleanTitle,
                SortTitle = game.SortTitle,
                Year = game.Year,
                Overview = game.Overview,
                IgdbId = game.IgdbId,
                SteamAppId = game.SteamAppId,
                Platform = game.Platform,
                Genres = game.Genres,
                Developer = game.Developer,
                Publisher = game.Publisher,
                ReleaseDate = game.ReleaseDate,
                Images = game.Images,
                Path = game.Path,
                Monitored = game.Monitored,
                QualityProfileId = game.QualityProfileId,
                Tags = game.Tags,
                Added = game.Added,
                RootFolderPath = game.RootFolderPath
            };

            // Set the remote cover URL for convenience
            var cover = game.Images?.Find(i => i.CoverType == MediaCoverTypes.Cover);
            if (cover != null)
            {
                resource.RemoteCover = cover.RemoteUrl;
            }

            return resource;
        }

        public Kagarr.Core.Games.Game ToModel()
        {
            return new Kagarr.Core.Games.Game
            {
                Id = Id,
                Title = Title,
                CleanTitle = CleanTitle,
                SortTitle = SortTitle,
                Year = Year,
                Overview = Overview,
                IgdbId = IgdbId,
                SteamAppId = SteamAppId,
                Platform = Platform,
                Genres = Genres ?? new List<string>(),
                Developer = Developer,
                Publisher = Publisher,
                ReleaseDate = ReleaseDate,
                Images = Images ?? new List<MediaCover>(),
                Path = Path,
                Monitored = Monitored,
                QualityProfileId = QualityProfileId,
                Tags = Tags ?? new HashSet<int>(),
                Added = Added,
                RootFolderPath = RootFolderPath
            };
        }
    }
}
