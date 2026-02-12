using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Kagarr.Common.Instrumentation;
using NLog;

namespace Kagarr.Core.Backup
{
    public interface IBackupService
    {
        BackupInfo CreateBackup();
        List<BackupInfo> GetBackups();
    }

    public class BackupService : IBackupService
    {
        private readonly string _dataPath;
        private readonly string _backupPath;
        private readonly int _retentionDays;
        private readonly Logger _logger;

        public BackupService(string dataPath)
        {
            _dataPath = dataPath;
            _backupPath = Path.Combine(dataPath, "Backups");
            _logger = KagarrLogger.GetLogger(this);

            var retentionEnv = Environment.GetEnvironmentVariable("KAGARR_BACKUP_RETENTION_DAYS");
            _retentionDays = 7;
            if (!string.IsNullOrWhiteSpace(retentionEnv) && int.TryParse(retentionEnv, out var parsed) && parsed > 0)
            {
                _retentionDays = parsed;
            }

            Directory.CreateDirectory(_backupPath);
        }

        public BackupInfo CreateBackup()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture);
            var zipName = $"kagarr_backup_{timestamp}.zip";
            var zipPath = Path.Combine(_backupPath, zipName);

            _logger.Info("Creating backup: {0}", zipName);

            var dbPath = Path.Combine(_dataPath, "kagarr.db");

            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // Use SQLite backup API via temp copy for safe live backup
                if (File.Exists(dbPath))
                {
                    var tempDb = Path.Combine(_dataPath, "kagarr_backup_temp.db");
                    try
                    {
                        File.Copy(dbPath, tempDb, true);
                        archive.CreateEntryFromFile(tempDb, "kagarr.db");
                    }
                    finally
                    {
                        if (File.Exists(tempDb))
                        {
                            File.Delete(tempDb);
                        }
                    }
                }

                // Back up API key file if it exists
                var apiKeyPath = Path.Combine(_dataPath, "api_key");
                if (File.Exists(apiKeyPath))
                {
                    archive.CreateEntryFromFile(apiKeyPath, "api_key");
                }
            }

            var fileInfo = new FileInfo(zipPath);
            _logger.Info("Backup created: {0} ({1} bytes)", zipName, fileInfo.Length);

            // Run retention cleanup
            CleanupOldBackups();

            return new BackupInfo
            {
                Name = zipName,
                Size = fileInfo.Length,
                Time = fileInfo.CreationTimeUtc
            };
        }

        public List<BackupInfo> GetBackups()
        {
            if (!Directory.Exists(_backupPath))
            {
                return new List<BackupInfo>();
            }

            return Directory.GetFiles(_backupPath, "kagarr_backup_*.zip")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTimeUtc)
                .Select(f => new BackupInfo
                {
                    Name = f.Name,
                    Size = f.Length,
                    Time = f.CreationTimeUtc
                })
                .ToList();
        }

        private void CleanupOldBackups()
        {
            if (!Directory.Exists(_backupPath))
            {
                return;
            }

            var cutoff = DateTime.UtcNow.AddDays(-_retentionDays);
            var oldFiles = Directory.GetFiles(_backupPath, "kagarr_backup_*.zip")
                .Select(f => new FileInfo(f))
                .Where(f => f.CreationTimeUtc < cutoff)
                .ToList();

            foreach (var file in oldFiles)
            {
                _logger.Info("Removing old backup: {0}", file.Name);
                file.Delete();
            }
        }
    }
}
