using Kagarr.Core.Datastore;

namespace Kagarr.Core.RemotePathMappings
{
    public interface IRemotePathMappingRepository : IBasicRepository<RemotePathMapping>
    {
    }

    public class RemotePathMappingRepository : BasicRepository<RemotePathMapping>, IRemotePathMappingRepository
    {
        public RemotePathMappingRepository(IDatabase database)
            : base(database)
        {
        }
    }
}
