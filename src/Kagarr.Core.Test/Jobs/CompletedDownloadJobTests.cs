using System;
using System.Collections.Generic;
using FluentAssertions;
using Kagarr.Core.Download;
using Kagarr.Core.History;
using Kagarr.Core.Jobs;
using Kagarr.Core.MediaFiles;
using Kagarr.Core.RemotePathMappings;
using Moq;
using NUnit.Framework;

namespace Kagarr.Core.Test.Jobs
{
    [TestFixture]
    public sealed class CompletedDownloadJobTests : IDisposable
    {
        private Mock<IDownloadClientService> _downloadClientService;
        private Mock<IDownloadTrackingRepository> _trackingRepo;
        private Mock<IImportGameFile> _importService;
        private Mock<IHistoryService> _historyService;
        private Mock<IRemotePathMappingService> _remotePathMappingService;
        private CompletedDownloadJob _job;

        [SetUp]
        public void Setup()
        {
            _downloadClientService = new Mock<IDownloadClientService>();
            _trackingRepo = new Mock<IDownloadTrackingRepository>();
            _importService = new Mock<IImportGameFile>();
            _historyService = new Mock<IHistoryService>();
            _remotePathMappingService = new Mock<IRemotePathMappingService>();

            _remotePathMappingService
                .Setup(s => s.RemapRemoteToLocal(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((host, path) => path);

            _job = new CompletedDownloadJob(
                _downloadClientService.Object,
                _trackingRepo.Object,
                _importService.Object,
                _historyService.Object,
                _remotePathMappingService.Object);
        }

        [TearDown]
        public void Dispose()
        {
            _job?.Dispose();
        }

        private DownloadClientItem GivenCompletedItem(string downloadId = "abc123")
        {
            var item = new DownloadClientItem
            {
                DownloadId = downloadId,
                Title = "Baldurs.Gate.3-RUNE",
                Status = DownloadItemStatus.Completed,
                OutputPath = "/nonexistent/downloads/bg3.iso",
                DownloadClientName = "qBittorrent"
            };

            _downloadClientService.Setup(s => s.GetQueue())
                .Returns(new List<DownloadClientItem> { item });

            return item;
        }

        private DownloadTracking GivenTracking(string downloadId = "abc123", int attempts = 0, DateTime? lastAttempt = null)
        {
            var tracking = new DownloadTracking
            {
                Id = 1,
                DownloadId = downloadId,
                GameId = 42,
                GameTitle = "Baldur's Gate 3",
                ImportAttempts = attempts,
                LastAttemptDate = lastAttempt
            };

            _trackingRepo.Setup(r => r.FindByDownloadId(downloadId)).Returns(tracking);

            return tracking;
        }

        private void GivenImportFails()
        {
            _importService.Setup(s => s.Import(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TransferMode>()))
                .Returns(new ImportResult { Success = false, Errors = new List<string> { "File not found" } });
        }

        [Test]
        public void Successful_import_should_delete_tracking_and_record_history()
        {
            GivenCompletedItem();
            var tracking = GivenTracking();

            _importService.Setup(s => s.Import(It.IsAny<string>(), 42, It.IsAny<TransferMode>()))
                .Returns(new ImportResult { Success = true });

            _job.ProcessCompletedDownloads();

            _trackingRepo.Verify(r => r.Delete(tracking), Times.Once);
            _historyService.Verify(
                h => h.RecordEvent(HistoryEventType.Imported, 42, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public void Completed_item_without_tracking_should_not_import()
        {
            GivenCompletedItem();
            _trackingRepo.Setup(r => r.FindByDownloadId(It.IsAny<string>())).Returns((DownloadTracking)null);

            _job.ProcessCompletedDownloads();

            _importService.Verify(s => s.Import(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TransferMode>()), Times.Never);
        }

        [Test]
        public void First_failed_import_should_record_history_and_increment_attempts()
        {
            GivenCompletedItem();
            var tracking = GivenTracking();
            GivenImportFails();

            _job.ProcessCompletedDownloads();

            tracking.ImportAttempts.Should().Be(1);
            tracking.LastAttemptDate.Should().NotBeNull();
            _trackingRepo.Verify(r => r.Update(tracking), Times.Once);
            _trackingRepo.Verify(r => r.Delete(It.IsAny<DownloadTracking>()), Times.Never);
            _historyService.Verify(
                h => h.RecordEvent(HistoryEventType.ImportFailed, 42, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public void Failed_import_within_backoff_window_should_be_skipped()
        {
            GivenCompletedItem();
            var tracking = GivenTracking(attempts: 1, lastAttempt: DateTime.UtcNow.AddMinutes(-5));
            GivenImportFails();

            _job.ProcessCompletedDownloads();

            _importService.Verify(s => s.Import(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TransferMode>()), Times.Never);
            tracking.ImportAttempts.Should().Be(1);
        }

        [Test]
        public void Failed_import_after_backoff_elapsed_should_retry_without_history_spam()
        {
            GivenCompletedItem();
            var tracking = GivenTracking(attempts: 2, lastAttempt: DateTime.UtcNow.AddMinutes(-31));
            GivenImportFails();

            _job.ProcessCompletedDownloads();

            tracking.ImportAttempts.Should().Be(3);
            _trackingRepo.Verify(r => r.Update(tracking), Times.Once);

            // Retries after the first failure must not append additional history rows
            _historyService.Verify(
                h => h.RecordEvent(HistoryEventType.ImportFailed, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public void Failed_import_should_give_up_after_max_attempts()
        {
            GivenCompletedItem();
            var tracking = GivenTracking(
                attempts: CompletedDownloadJob.MaxImportAttempts - 1,
                lastAttempt: DateTime.UtcNow.AddHours(-24));
            GivenImportFails();

            _job.ProcessCompletedDownloads();

            tracking.ImportAttempts.Should().Be(CompletedDownloadJob.MaxImportAttempts);
            _trackingRepo.Verify(r => r.Delete(tracking), Times.Once);
            _trackingRepo.Verify(r => r.Update(It.IsAny<DownloadTracking>()), Times.Never);
            _historyService.Verify(
                h => h.RecordEvent(HistoryEventType.ImportFailed, 42, It.IsAny<string>(), It.IsAny<string>(), It.Is<string>(d => d.Contains("Giving up"))),
                Times.Once);
        }
    }
}
