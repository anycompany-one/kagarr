using System;
using System.Threading;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Backup;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Kagarr.Core.Jobs
{
    public class BackupJob : BackgroundService
    {
        private readonly IBackupService _backupService;
        private readonly Logger _logger;
        private readonly TimeSpan _interval;

        public BackupJob(IBackupService backupService)
        {
            _backupService = backupService;
            _logger = KagarrLogger.GetLogger(this);

            var hoursEnv = global::System.Environment.GetEnvironmentVariable("KAGARR_BACKUP_INTERVAL_HOURS");
            var hours = 24;
            if (!string.IsNullOrWhiteSpace(hoursEnv) && int.TryParse(hoursEnv, out var parsed) && parsed > 0)
            {
                hours = parsed;
            }

            _interval = TimeSpan.FromHours(hours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info("Backup job started. Interval: {0}h", _interval.TotalHours);

            // Wait 5 minutes after startup before first backup
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _backupService.CreateBackup();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Scheduled backup failed");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
