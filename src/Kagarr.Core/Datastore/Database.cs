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
        private bool _walEnabled;

        public MainDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection OpenConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Enable WAL mode for safe concurrent reads/writes from background jobs + API
            if (!_walEnabled)
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA journal_mode=WAL;";
                    cmd.ExecuteNonQuery();
                }

                _walEnabled = true;
            }

            return connection;
        }
    }
}
