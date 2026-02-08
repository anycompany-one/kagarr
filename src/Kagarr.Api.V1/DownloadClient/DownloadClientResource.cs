using Kagarr.Core.Download;
using Kagarr.Http.REST;

namespace Kagarr.Api.V1.DownloadClient
{
    public class DownloadClientResource : RestResource
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string Settings { get; set; }
        public string Protocol { get; set; }
        public int Priority { get; set; }
        public bool Enable { get; set; }

        public static DownloadClientResource FromModel(DownloadClientDefinition model)
        {
            if (model == null)
            {
                return null;
            }

            return new DownloadClientResource
            {
                Id = model.Id,
                Name = model.Name,
                Implementation = model.Implementation,
                Settings = model.Settings,
                Protocol = model.Protocol,
                Priority = model.Priority,
                Enable = model.Enable
            };
        }

        public DownloadClientDefinition ToModel()
        {
            return new DownloadClientDefinition
            {
                Id = Id,
                Name = Name,
                Implementation = Implementation,
                Settings = Settings,
                Protocol = Protocol,
                Priority = Priority,
                Enable = Enable
            };
        }
    }
}
