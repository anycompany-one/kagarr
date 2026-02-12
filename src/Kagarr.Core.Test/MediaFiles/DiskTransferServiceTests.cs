using System;
using System.IO;
using FluentAssertions;
using Kagarr.Core.MediaFiles;
using NUnit.Framework;

namespace Kagarr.Core.Test.MediaFiles
{
    [TestFixture]
    public class DiskTransferServiceTests
    {
        private DiskTransferService _service;
        private string _tempDir;

        [SetUp]
        public void Setup()
        {
            _service = new DiskTransferService();
            _tempDir = Path.Combine(Path.GetTempPath(), "kagarr_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        [Test]
        public void TransferFile_copy_should_create_copy_and_keep_source()
        {
            var source = CreateTestFile("source.bin", "test content");
            var target = Path.Combine(_tempDir, "sub", "target.bin");

            var result = _service.TransferFile(source, target, TransferMode.Copy);

            result.Should().Be(TransferMode.Copy);
            File.Exists(target).Should().BeTrue();
            File.Exists(source).Should().BeTrue("source should still exist after copy");
            File.ReadAllText(target).Should().Be("test content");
        }

        [Test]
        public void TransferFile_move_should_move_file()
        {
            var source = CreateTestFile("source.bin", "move content");
            var target = Path.Combine(_tempDir, "moved.bin");

            var result = _service.TransferFile(source, target, TransferMode.Move);

            result.Should().Be(TransferMode.Move);
            File.Exists(target).Should().BeTrue();
            File.Exists(source).Should().BeFalse("source should be removed after move");
            File.ReadAllText(target).Should().Be("move content");
        }

        [Test]
        public void TransferFile_hardlink_or_copy_should_succeed()
        {
            var source = CreateTestFile("source.bin", "hardlink test");
            var target = Path.Combine(_tempDir, "linked.bin");

            var result = _service.TransferFile(source, target, TransferMode.HardLinkOrCopy);

            // Should succeed with either hardlink or copy fallback
            result.Should().BeOneOf(TransferMode.HardLink, TransferMode.Copy);
            File.Exists(target).Should().BeTrue();
            File.Exists(source).Should().BeTrue("source should still exist");
            File.ReadAllText(target).Should().Be("hardlink test");
        }

        [Test]
        public void TransferFile_should_overwrite_existing_target()
        {
            var source = CreateTestFile("source.bin", "new content");
            var target = CreateTestFile("target.bin", "old content");

            _service.TransferFile(source, target, TransferMode.Copy);

            File.ReadAllText(target).Should().Be("new content");
        }

        [Test]
        public void TransferFile_with_missing_source_should_throw()
        {
            var target = Path.Combine(_tempDir, "target.bin");

            var act = () => _service.TransferFile("/nonexistent/file.bin", target, TransferMode.Move);

            act.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void TransferFile_with_empty_source_should_throw()
        {
            var target = Path.Combine(_tempDir, "target.bin");

            var act = () => _service.TransferFile(string.Empty, target, TransferMode.Move);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void TransferFile_with_none_mode_should_throw()
        {
            var source = CreateTestFile("source.bin", "content");
            var target = Path.Combine(_tempDir, "target.bin");

            var act = () => _service.TransferFile(source, target, TransferMode.None);

            act.Should().Throw<ArgumentException>();
        }

        private string CreateTestFile(string name, string content)
        {
            var path = Path.Combine(_tempDir, name);
            File.WriteAllText(path, content);
            return path;
        }
    }
}
