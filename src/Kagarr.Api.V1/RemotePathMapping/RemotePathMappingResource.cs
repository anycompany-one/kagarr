namespace Kagarr.Api.V1.RemotePathMapping
{
    public class RemotePathMappingResource
    {
        public int Id { get; set; }
        public string Host { get; set; }
        public string RemotePath { get; set; }
        public string LocalPath { get; set; }

        public Core.RemotePathMappings.RemotePathMapping ToModel()
        {
            return new Core.RemotePathMappings.RemotePathMapping
            {
                Id = Id,
                Host = Host,
                RemotePath = RemotePath,
                LocalPath = LocalPath
            };
        }

        public static RemotePathMappingResource FromModel(Core.RemotePathMappings.RemotePathMapping model)
        {
            return new RemotePathMappingResource
            {
                Id = model.Id,
                Host = model.Host,
                RemotePath = model.RemotePath,
                LocalPath = model.LocalPath
            };
        }
    }
}
