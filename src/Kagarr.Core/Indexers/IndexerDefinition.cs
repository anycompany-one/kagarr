using Kagarr.Core.Datastore;

namespace Kagarr.Core.Indexers
{
    public class IndexerDefinition : ModelBase
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string Settings { get; set; }
        public bool EnableRss { get; set; }
        public bool EnableSearch { get; set; }
        public int Priority { get; set; } = 25;
    }
}
