using System.Net.Http;
using FluentAssertions;
using Kagarr.Core.Games;
using Kagarr.Core.MediaCovers;
using Moq;
using NUnit.Framework;

namespace Kagarr.Core.Test.MediaCovers
{
    [TestFixture]
    public class MediaCoverServiceTests
    {
        private string _dataPath;
        private MediaCoverService _service;

        [SetUp]
        public void Setup()
        {
            _dataPath = global::System.IO.Path.Combine(global::System.IO.Path.GetTempPath(), "kagarr-mediacover-tests-" + global::System.Guid.NewGuid().ToString("N"));

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(() => new HttpClient());

            _service = new MediaCoverService(_dataPath, httpClientFactory.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (global::System.IO.Directory.Exists(_dataPath))
            {
                global::System.IO.Directory.Delete(_dataPath, true);
            }
        }

        [Test]
        public void should_store_covers_under_the_configured_data_path()
        {
            var path = _service.GetCoverPath(42, MediaCoverTypes.Cover);

            path.Should().Be(global::System.IO.Path.Combine(_dataPath, "MediaCover", "42", "cover.jpg"));
        }

        [Test]
        public void GetStoredCoverPath_should_return_path_for_existing_cover()
        {
            var coverPath = _service.GetCoverPath(42, MediaCoverTypes.Cover);
            global::System.IO.File.WriteAllBytes(coverPath, new byte[] { 1, 2, 3 });

            var result = _service.GetStoredCoverPath(42, "cover.jpg");

            result.Should().Be(coverPath);
        }

        [Test]
        public void GetStoredCoverPath_should_return_null_for_missing_file()
        {
            _service.GetStoredCoverPath(42, "cover.jpg").Should().BeNull();
        }

        [TestCase("../secrets.txt")]
        [TestCase("..\\secrets.txt")]
        [TestCase("foo/../../secrets.txt")]
        [TestCase("/etc/passwd")]
        [TestCase("subdir/cover.jpg")]
        [TestCase("subdir\\cover.jpg")]
        [TestCase("..")]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void GetStoredCoverPath_should_reject_unsafe_file_names(string fileName)
        {
            _service.GetStoredCoverPath(42, fileName).Should().BeNull();
        }

        [TestCase("cover.jpg", true)]
        [TestCase("screenshot.png", true)]
        [TestCase("../cover.jpg", false)]
        [TestCase("a/../b.jpg", false)]
        [TestCase("C:\\windows\\system32", false)]
        public void IsSafeFileName_should_validate(string fileName, bool expected)
        {
            MediaCoverService.IsSafeFileName(fileName).Should().Be(expected);
        }
    }
}
