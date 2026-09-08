using System.IO;
using FluentAssertions;
using Kagarr.Core.Backup;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace Kagarr.Core.Test.Backup
{
    [TestFixture]
    public class SqliteDatabaseBackupTests
    {
        private string _tempDir;
        private string _sourcePath;
        private string _destPath;

        [SetUp]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "kagarr_backup_test_" + Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
            _sourcePath = Path.Combine(_tempDir, "kagarr.db");
            _destPath = Path.Combine(_tempDir, "kagarr_copy.db");
        }

        [TearDown]
        public void TearDown()
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        [Test]
        public void BackupTo_should_include_wal_contents_while_source_connection_is_open()
        {
            using (var source = new SqliteConnection($"Data Source={_sourcePath};Pooling=False"))
            {
                source.Open();
                Execute(source, "PRAGMA journal_mode=WAL;");
                Execute(source, "CREATE TABLE Games (Id INTEGER PRIMARY KEY, Title TEXT)");
                Execute(source, "INSERT INTO Games (Title) VALUES ('Baldurs Gate 3'), ('Hades II')");

                // Rows are still in the WAL (not checkpointed) and the source connection stays open
                SqliteDatabaseBackup.BackupTo(_sourcePath, _destPath);
            }

            File.Exists(_destPath).Should().BeTrue();

            using (var copy = new SqliteConnection($"Data Source={_destPath};Pooling=False"))
            {
                copy.Open();
                using var cmd = copy.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Games";
                var count = (long)cmd.ExecuteScalar();
                count.Should().Be(2);
            }
        }

        [Test]
        public void BackupTo_should_overwrite_existing_destination()
        {
            using (var source = new SqliteConnection($"Data Source={_sourcePath};Pooling=False"))
            {
                source.Open();
                Execute(source, "CREATE TABLE Games (Id INTEGER PRIMARY KEY, Title TEXT)");
                Execute(source, "INSERT INTO Games (Title) VALUES ('Celeste')");
            }

            File.WriteAllText(_destPath, "stale garbage that is not a database");

            SqliteDatabaseBackup.BackupTo(_sourcePath, _destPath);

            using (var copy = new SqliteConnection($"Data Source={_destPath};Pooling=False"))
            {
                copy.Open();
                using var cmd = copy.CreateCommand();
                cmd.CommandText = "SELECT Title FROM Games";
                cmd.ExecuteScalar().Should().Be("Celeste");
            }
        }

        private static void Execute(SqliteConnection connection, string sql)
        {
            using var cmd = connection.CreateCommand();
#pragma warning disable CA2100 // Test-only helper executing constant SQL
            cmd.CommandText = sql;
#pragma warning restore CA2100
            cmd.ExecuteNonQuery();
        }
    }
}
