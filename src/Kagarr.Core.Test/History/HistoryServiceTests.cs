using System;
using System.Collections.Generic;
using FluentAssertions;
using Kagarr.Core.History;
using Moq;
using NUnit.Framework;

namespace Kagarr.Core.Test.History
{
    [TestFixture]
    public class HistoryServiceTests
    {
        private Mock<IHistoryRepository> _historyRepo;
        private HistoryService _historyService;

        [SetUp]
        public void Setup()
        {
            _historyRepo = new Mock<IHistoryRepository>();
            _historyService = new HistoryService(_historyRepo.Object);
        }

        [Test]
        public void RecordEvent_should_insert_record_with_correct_fields()
        {
            _historyRepo.Setup(r => r.Insert(It.IsAny<HistoryRecord>()))
                .Returns<HistoryRecord>(h =>
                {
                    h.Id = 1;
                    return h;
                });

            _historyService.RecordEvent(
                HistoryEventType.Grabbed,
                42,
                "Baldur's Gate 3",
                "Baldurs.Gate.3-RUNE",
                "Sent to qBittorrent");

            _historyRepo.Verify(r => r.Insert(It.Is<HistoryRecord>(h =>
                h.EventType == "Grabbed" &&
                h.GameId == 42 &&
                h.GameTitle == "Baldur's Gate 3" &&
                h.SourceTitle == "Baldurs.Gate.3-RUNE" &&
                h.Data == "Sent to qBittorrent" &&
                h.Date > DateTime.UtcNow.AddMinutes(-1))),
                Times.Once);
        }

        [Test]
        public void GetRecent_should_delegate_to_repository()
        {
            var records = new List<HistoryRecord>
            {
                new HistoryRecord
                {
                    Id = 1,
                    EventType = HistoryEventType.Grabbed,
                    GameId = 1,
                    GameTitle = "Game A",
                    Date = DateTime.UtcNow
                },
                new HistoryRecord
                {
                    Id = 2,
                    EventType = HistoryEventType.Imported,
                    GameId = 1,
                    GameTitle = "Game A",
                    Date = DateTime.UtcNow.AddMinutes(-5)
                }
            };

            _historyRepo.Setup(r => r.GetRecent(50)).Returns(records);

            var result = _historyService.GetRecent();

            result.Should().HaveCount(2);
            result[0].EventType.Should().Be("Grabbed");
        }

        [Test]
        public void GetByGameId_should_delegate_to_repository()
        {
            var records = new List<HistoryRecord>
            {
                new HistoryRecord { Id = 1, GameId = 42, EventType = HistoryEventType.Imported }
            };

            _historyRepo.Setup(r => r.FindByGameId(42)).Returns(records);

            var result = _historyService.GetByGameId(42);

            result.Should().HaveCount(1);
            result[0].GameId.Should().Be(42);
        }
    }
}
