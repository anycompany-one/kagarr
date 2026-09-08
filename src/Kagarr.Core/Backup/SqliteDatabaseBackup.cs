using System.IO;
using Microsoft.Data.Sqlite;

namespace Kagarr.Core.Backup
{
    /// <summary>
    /// Creates a consistent copy of a live SQLite database using the SQLite online backup API.
    /// Unlike File.Copy, this includes WAL contents and cannot produce a torn copy.
    /// </summary>
    public static class SqliteDatabaseBackup
    {
        public static void BackupTo(string sourceDbPath, string destinationDbPath)
        {
            if (File.Exists(destinationDbPath))
            {
                File.Delete(destinationDbPath);
            }

            using (var source = new SqliteConnection($"Data Source={sourceDbPath};Mode=ReadOnly;Pooling=False"))
            using (var destination = new SqliteConnection($"Data Source={destinationDbPath};Pooling=False"))
            {
                source.Open();
                destination.Open();
                source.BackupDatabase(destination);
            }
        }
    }
}
