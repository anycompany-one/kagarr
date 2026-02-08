using Kagarr.Core.Datastore;

namespace Kagarr.Core.Download
{
    public class DownloadClientDefinition : ModelBase
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string Settings { get; set; }
        public string Protocol { get; set; }
        public int Priority { get; set; } = 1;
        public bool Enable { get; set; } = true;
    }
}
