using Kagarr.Core.Indexers;
using Kagarr.Http.REST;

namespace Kagarr.Api.V1.Indexers
{
    public class IndexerResource : RestResource
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string Settings { get; set; }
        public bool EnableRss { get; set; }
        public bool EnableSearch { get; set; }
        public int Priority { get; set; }

        public static IndexerResource FromModel(IndexerDefinition model)
        {
            if (model == null)
            {
                return null;
            }

            return new IndexerResource
            {
                Id = model.Id,
                Name = model.Name,
                Implementation = model.Implementation,
                Settings = model.Settings,
                EnableRss = model.EnableRss,
                EnableSearch = model.EnableSearch,
                Priority = model.Priority
            };
        }

        public IndexerDefinition ToModel()
        {
            return new IndexerDefinition
            {
                Id = Id,
                Name = Name,
                Implementation = Implementation,
                Settings = Settings,
                EnableRss = EnableRss,
                EnableSearch = EnableSearch,
                Priority = Priority
            };
        }
    }
}
