using System;
using Kagarr.Core.Platforms;
using Kagarr.Http.REST;

namespace Kagarr.Api.V1.GameFile
{
    public class GameFileResource : RestResource
    {
        public int GameId { get; set; }
        public string RelativePath { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string Quality { get; set; }
        public GamePlatform Platform { get; set; }
        public string ReleaseGroup { get; set; }

        public static GameFileResource FromModel(Kagarr.Core.Games.GameFile model)
        {
            if (model == null)
            {
                return null;
            }

            return new GameFileResource
            {
                Id = model.Id,
                GameId = model.GameId,
                RelativePath = model.RelativePath,
                Size = model.Size,
                DateAdded = model.DateAdded,
                Quality = model.Quality,
                Platform = model.Platform,
                ReleaseGroup = model.ReleaseGroup
            };
        }
    }
}
