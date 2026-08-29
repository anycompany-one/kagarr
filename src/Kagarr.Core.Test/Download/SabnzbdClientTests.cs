using FluentAssertions;
using Kagarr.Core.Download;
using Kagarr.Core.Download.Clients.Sabnzbd;
using NUnit.Framework;

namespace Kagarr.Core.Test.Download
{
    [TestFixture]
    public class SabnzbdClientTests
    {
        [TestCase("Completed")]
        [TestCase("completed")]
        public void MapHistoryStatus_completed_should_map_to_completed(string status)
        {
            SabnzbdClient.MapHistoryStatus(status).Should().Be(DownloadItemStatus.Completed);
        }

        [TestCase("Failed")]
        [TestCase("failed")]
        public void MapHistoryStatus_failed_should_map_to_failed(string status)
        {
            SabnzbdClient.MapHistoryStatus(status).Should().Be(DownloadItemStatus.Failed);
        }

        [TestCase("Verifying")]
        [TestCase("Repairing")]
        [TestCase("Extracting")]
        [TestCase("Running")]
        [TestCase("Queued")]
        [TestCase("SomeUnknownFutureStatus")]
        [TestCase("")]
        [TestCase(null)]
        public void MapHistoryStatus_other_statuses_should_map_to_downloading(string status)
        {
            SabnzbdClient.MapHistoryStatus(status).Should().Be(DownloadItemStatus.Downloading);
        }
    }
}
