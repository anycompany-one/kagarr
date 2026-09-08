using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Kagarr.Api.V1.MediaCovers;
using Kagarr.Core.Games;
using Kagarr.Core.MediaCovers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Kagarr.Host.Test.MediaCover
{
    [TestFixture]
    public sealed class MediaCoverControllerTests : System.IDisposable
    {
        private string _dataPath;
        private FakeCoverMapper _coverMapper;
        private MediaCoverController _controller;

        [SetUp]
        public void Setup()
        {
            _dataPath = global::System.IO.Path.Combine(global::System.IO.Path.GetTempPath(), "kagarr-mediacover-controller-tests-" + global::System.Guid.NewGuid().ToString("N"));
            global::System.IO.Directory.CreateDirectory(global::System.IO.Path.Combine(_dataPath, "MediaCover", "42"));

            _coverMapper = new FakeCoverMapper(_dataPath);
            _controller = new MediaCoverController(_coverMapper);
        }

        [TearDown]
        public void Dispose()
        {
            _controller?.Dispose();

            if (global::System.IO.Directory.Exists(_dataPath))
            {
                global::System.IO.Directory.Delete(_dataPath, true);
            }
        }

        [Test]
        public void should_serve_existing_cover_as_physical_file_with_content_type()
        {
            var coverPath = global::System.IO.Path.Combine(_dataPath, "MediaCover", "42", "cover.jpg");
            global::System.IO.File.WriteAllBytes(coverPath, new byte[] { 0xFF, 0xD8, 0xFF });

            var result = _controller.GetMediaCover(42, "cover.jpg");

            var fileResult = result.Should().BeOfType<PhysicalFileResult>().Subject;
            fileResult.FileName.Should().Be(coverPath);
            fileResult.ContentType.Should().Be("image/jpeg");
        }

        [Test]
        public void should_return_not_found_for_missing_cover()
        {
            var result = _controller.GetMediaCover(42, "cover.jpg");

            result.Should().BeOfType<NotFoundResult>();
        }

        [TestCase("../../kagarr.db")]
        [TestCase("..\\..\\kagarr.db")]
        [TestCase("foo/../../kagarr.db")]
        [TestCase("/etc/passwd")]
        [TestCase("..")]
        public void should_return_not_found_for_path_traversal_attempts(string fileName)
        {
            // Even with a sensitive file present next to the cover folder,
            // traversal attempts must never resolve to it.
            var dbPath = global::System.IO.Path.Combine(_dataPath, "kagarr.db");
            global::System.IO.File.WriteAllBytes(dbPath, new byte[] { 1 });

            var result = _controller.GetMediaCover(42, fileName);

            result.Should().BeOfType<NotFoundResult>();
        }

        /// <summary>
        /// Wraps the real MediaCoverService validation logic against a temp data path.
        /// </summary>
        private sealed class FakeCoverMapper : IMapCoversToLocal
        {
            private readonly string _rootFolder;

            public FakeCoverMapper(string dataPath)
            {
                _rootFolder = global::System.IO.Path.Combine(dataPath, "MediaCover");
            }

            public Task ConvertToLocalUrlsAsync(int gameId, IEnumerable<Kagarr.Core.Games.MediaCover> covers)
            {
                return Task.CompletedTask;
            }

            public string GetCoverPath(int gameId, MediaCoverTypes coverType)
            {
                return global::System.IO.Path.Combine(_rootFolder, gameId.ToString(), coverType.ToString().ToLowerInvariant() + ".jpg");
            }

            public string GetStoredCoverPath(int gameId, string fileName)
            {
                if (!MediaCoverService.IsSafeFileName(fileName))
                {
                    return null;
                }

                var path = global::System.IO.Path.Combine(_rootFolder, gameId.ToString(), fileName);
                return global::System.IO.File.Exists(path) ? path : null;
            }
        }
    }
}
