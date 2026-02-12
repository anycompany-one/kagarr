using FluentAssertions;
using Kagarr.Core.MediaFiles;
using NUnit.Framework;

namespace Kagarr.Core.Test.MediaFiles
{
    [TestFixture]
    public class GameFileDetectorTests
    {
        [Test]
        public void DetectFileType_iso_should_return_Iso()
        {
            var result = GameFileDetector.DetectFileType("/games/test.iso");
            result.Should().Be(GameFileType.Iso);
        }

        [Test]
        public void DetectFileType_exe_should_return_Installer()
        {
            var result = GameFileDetector.DetectFileType("/games/setup.exe");
            result.Should().Be(GameFileType.Installer);
        }

        [Test]
        public void DetectFileType_zip_should_return_Compressed()
        {
            var result = GameFileDetector.DetectFileType("/games/archive.zip");
            result.Should().Be(GameFileType.Compressed);
        }

        [Test]
        public void DetectFileType_nsp_should_return_Rom()
        {
            var result = GameFileDetector.DetectFileType("/games/switch-game.nsp");
            result.Should().Be(GameFileType.Rom);
        }

        [Test]
        public void DetectFileType_unknown_extension_should_return_Unknown()
        {
            var result = GameFileDetector.DetectFileType("/games/readme.txt");
            result.Should().Be(GameFileType.Unknown);
        }

        [Test]
        public void IsGameFile_should_return_true_for_game_extensions()
        {
            GameFileDetector.IsGameFile("/games/test.iso").Should().BeTrue();
            GameFileDetector.IsGameFile("/games/test.exe").Should().BeTrue();
            GameFileDetector.IsGameFile("/games/test.7z").Should().BeTrue();
            GameFileDetector.IsGameFile("/games/test.nsp").Should().BeTrue();
        }

        [Test]
        public void IsGameFile_should_return_false_for_non_game_extensions()
        {
            GameFileDetector.IsGameFile("/games/readme.txt").Should().BeFalse();
            GameFileDetector.IsGameFile("/games/cover.jpg").Should().BeFalse();
            GameFileDetector.IsGameFile("/games/data.json").Should().BeFalse();
        }

        [Test]
        public void ScanDirectory_nonexistent_path_should_return_empty()
        {
            var result = GameFileDetector.ScanDirectory("/absolutely/nonexistent/path");
            result.Should().BeEmpty();
        }
    }
}
