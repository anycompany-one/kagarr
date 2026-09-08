using System;
using System.IO;
using FluentAssertions;
using Kagarr.Core.Datastore;
using Kagarr.Core.Download;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace Kagarr.Core.Test.Download
{
    [TestFixture]
    public class DownloadTrackingRepositoryTests
    {
        private string _tempDir;
        private MainDatabase _database;
        private DownloadTrackingRepository _repository;

        [SetUp]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "kagarr_repo_test_" + Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
            var dbPath = Path.Combine(_tempDir, "kagarr.db");

            _database = new MainDatabase($"Data Source={dbPath};Pooling=False");

            using (var conn = _database.OpenConnection())
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"CREATE TABLE ""DownloadTrackings"" (
                    ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""DownloadId"" TEXT NOT NULL,
                    ""GameId"" INTEGER NOT NULL,
                    ""GameTitle"" TEXT,
                    ""SourceTitle"" TEXT,
                    ""AddedDate"" TEXT NOT NULL,
                    ""ImportAttempts"" INTEGER NOT NULL DEFAULT 0,
                    ""LastAttemptDate"" TEXT)";
                cmd.ExecuteNonQuery();
            }

            _repository = new DownloadTrackingRepository(_database);
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

        private DownloadTracking GivenTracking(string downloadId, int gameId = 1)
        {
            return _repository.Insert(new DownloadTracking
            {
                DownloadId = downloadId,
                GameId = gameId,
                GameTitle = "Game " + gameId,
                SourceTitle = "Source " + gameId,
                AddedDate = DateTime.UtcNow
            });
        }

        [Test]
        public void FindByDownloadId_should_return_matching_record()
        {
            GivenTracking("aaa", 1);
            GivenTracking("bbb", 2);

            var result = _repository.FindByDownloadId("bbb");

            result.Should().NotBeNull();
            result.GameId.Should().Be(2);
        }

        [Test]
        public void FindByDownloadId_should_return_null_when_not_found()
        {
            GivenTracking("aaa", 1);

            _repository.FindByDownloadId("missing").Should().BeNull();
        }

        [Test]
        public void FindByDownloadId_should_not_throw_on_duplicate_download_ids()
        {
            GivenTracking("dupe", 1);
            GivenTracking("dupe", 2);

            var act = () => _repository.FindByDownloadId("dupe");

            act.Should().NotThrow();
            act().Should().NotBeNull();
            act().GameId.Should().Be(1);
        }
    }
}
