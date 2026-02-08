using System.Data;

namespace Kagarr.Core.Datastore
{
    public interface IDatabase
    {
        IDbConnection OpenConnection();
    }
}
