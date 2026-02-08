using System.Data;
using Microsoft.Data.Sqlite;

namespace Kagarr.Core.Datastore
{
    public interface IMainDatabase : IDatabase
    {
    }

    public class MainDatabase : IMainDatabase
    {
        private readonly string _connectionString;

        public MainDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection OpenConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }
}
