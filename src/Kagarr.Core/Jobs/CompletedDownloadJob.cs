using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Download;
using Kagarr.Core.History;
using Kagarr.Core.MediaFiles;
using Kagarr.Core.RemotePathMappings;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Kagarr.Core.Jobs
{
    public class CompletedDownloadJob : BackgroundService
    {
        private readonly IDownloadClientService _downloadClientService;
        private readonly IDownloadTrackingRepository _trackingRepository;
        private readonly IImportGameFile _importService;
        private readonly IHistoryService _historyService;
        private readonly IRemotePathMappingService _remotePathMappingService;
        private readonly Logger _logger;
        private readonly TimeSpan _interval;

        public CompletedDownloadJob(
            IDownloadClientService downloadClientService,
            IDownloadTrackingRepository trackingRepository,
            IImportGameFile importService,
            IHistoryService historyService,
            IRemotePathMappingService remotePathMappingService)
        {
            _downloadClientService = downloadClientService;
            _trackingRepository = trackingRepository;
            _importService = importService;
            _historyService = historyService;
            _remotePathMappingService = remotePathMappingService;
            _logger = KagarrLogger.GetLogger(this);

            var secondsEnv = global::System.Environment.GetEnvironmentVariable("KAGARR_IMPORT_CHECK_SECONDS");
            var seconds = 60;
            if (!string.IsNullOrWhiteSpace(secondsEnv) && int.TryParse(secondsEnv, out var parsed) && parsed > 0)
            {
                seconds = parsed;
            }

            _interval = TimeSpan.FromSeconds(seconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info("Completed download import job started. Interval: {0}s", _interval.TotalSeconds);

            // Wait 1 minute after startup before the first check
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ProcessCompletedDownloads();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Completed download check failed");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private void ProcessCompletedDownloads()
        {
            var queue = _downloadClientService.GetQueue();
            var completedItems = queue.Where(i => i.Status == DownloadItemStatus.Completed).ToList();

            if (completedItems.Count == 0)
            {
                return;
            }

            _logger.Info("Found {0} completed download(s), checking for tracked items", completedItems.Count);

            foreach (var item in completedItems)
            {
                try
                {
                    ProcessItem(item);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error processing completed download '{0}'", item.Title);
                }
            }
        }

        private void ProcessItem(DownloadClientItem item)
        {
            var tracking = _trackingRepository.FindByDownloadId(item.DownloadId);
            if (tracking == null)
            {
                // Not tracked by Kagarr (user may have added it manually to the client)
                return;
            }

            _logger.Info("Auto-importing completed download '{0}' for game '{1}'", item.Title, tracking.GameTitle);

            if (string.IsNullOrWhiteSpace(item.OutputPath))
            {
                _logger.Warn("Completed download '{0}' has no output path, skipping", item.Title);
                return;
            }

            // Remap output path if remote path mappings are configured
            var outputPath = _remotePathMappingService.RemapRemoteToLocal(item.DownloadClientHost, item.OutputPath);
            if (!string.Equals(outputPath, item.OutputPath, StringComparison.Ordinal))
            {
                _logger.Info("Remapped output path from '{0}' to '{1}'", item.OutputPath, outputPath);
            }

            // Auto mode: torrent clients need hardlink-or-copy to keep seeding, usenet uses move
            var transferMode = string.Equals(item.DownloadProtocol, "torrent", StringComparison.OrdinalIgnoreCase)
                ? TransferMode.HardLinkOrCopy
                : TransferMode.Move;

            // Try folder import first (most downloads extract to folders), fall back to single file
            var results = global::System.IO.Directory.Exists(outputPath)
                ? _importService.ImportFolder(outputPath, tracking.GameId, transferMode)
                : new global::System.Collections.Generic.List<ImportResult> { _importService.Import(outputPath, tracking.GameId, transferMode) };

            var anySuccess = results.Any(r => r.Success);

            if (anySuccess)
            {
                _logger.Info("Successfully auto-imported '{0}' for game '{1}'", item.Title, tracking.GameTitle);
                _historyService.RecordEvent(
                    HistoryEventType.Imported,
                    tracking.GameId,
                    tracking.GameTitle,
                    item.Title,
                    $"Auto-imported from {item.DownloadClientName}");

                // Remove tracking record so we don't re-import
                _trackingRepository.Delete(tracking);
            }
            else
            {
                var errors = string.Join("; ", results.SelectMany(r => r.Errors));
                _logger.Warn("Auto-import failed for '{0}': {1}", item.Title, errors);
                _historyService.RecordEvent(
                    HistoryEventType.ImportFailed,
                    tracking.GameId,
                    tracking.GameTitle,
                    item.Title,
                    errors);
            }
        }
    }
}
