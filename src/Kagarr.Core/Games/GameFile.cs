using System;
using Kagarr.Core.Datastore;
using Kagarr.Core.Platforms;

namespace Kagarr.Core.Games
{
    public class GameFile : ModelBase
    {
        public int GameId { get; set; }
        public string RelativePath { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string Quality { get; set; }
        public GamePlatform Platform { get; set; }
        public string ReleaseGroup { get; set; }
    }
}
