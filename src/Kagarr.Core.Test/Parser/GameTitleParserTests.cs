using FluentAssertions;
using Kagarr.Core.Parser;
using NUnit.Framework;

namespace Kagarr.Core.Test.Parser
{
    [TestFixture]
    public class GameTitleParserTests
    {
        [Test]
        public void ParseTitle_scene_dots_with_group_should_extract_title_and_group()
        {
            var result = GameTitleParser.ParseTitle("Baldurs.Gate.3-RELOADED");
            result.CleanTitle.Should().Be("Baldurs Gate 3");
            result.ReleaseGroup.Should().Be("RELOADED");
        }

        [Test]
        public void ParseTitle_underscores_with_version_and_group_should_extract_all()
        {
            var result = GameTitleParser.ParseTitle("Cyberpunk_2077_v1.6-GOG");
            result.CleanTitle.Should().Contain("Cyberpunk");
            result.ReleaseGroup.Should().Be("GOG");
            result.Version.Should().Be("1.6");
        }

        [Test]
        public void ParseTitle_with_year_should_extract_year()
        {
            var result = GameTitleParser.ParseTitle("The.Witcher.3.2015.PC-FitGirl");
            result.Year.Should().Be(2015);
            result.ReleaseGroup.Should().Be("FitGirl");
        }

        [Test]
        public void ParseTitle_with_platform_in_brackets_should_extract_platform()
        {
            var result = GameTitleParser.ParseTitle("Elden.Ring.[PC]-CODEX");
            result.Platform.Should().Be("PC");
            result.ReleaseGroup.Should().Be("CODEX");
        }

        [Test]
        public void ParseTitle_plain_title_no_group_should_return_clean_title()
        {
            var result = GameTitleParser.ParseTitle("Stardew Valley");
            result.CleanTitle.Should().Be("Stardew Valley");
            result.ReleaseGroup.Should().BeNull();
        }

        [Test]
        public void ParseTitle_empty_string_should_return_empty_clean_title()
        {
            var result = GameTitleParser.ParseTitle(string.Empty);
            result.CleanTitle.Should().BeEmpty();
        }

        [Test]
        public void ParseTitle_null_should_return_empty_clean_title()
        {
            var result = GameTitleParser.ParseTitle(null);
            result.CleanTitle.Should().BeEmpty();
        }

        [Test]
        public void ParseTitle_should_preserve_original_title()
        {
            var original = "Baldurs.Gate.3-RELOADED";
            var result = GameTitleParser.ParseTitle(original);
            result.OriginalTitle.Should().Be(original);
        }

        [Test]
        public void ParseTitle_complex_version_should_extract()
        {
            var result = GameTitleParser.ParseTitle("Game.Name.v2.1.3-PLAZA");
            result.Version.Should().Be("2.1.3");
            result.ReleaseGroup.Should().Be("PLAZA");
        }

        [Test]
        public void ParseTitle_switch_platform_should_extract()
        {
            var result = GameTitleParser.ParseTitle("Mario.Kart.[Switch]-Scene");
            result.Platform.Should().Be("SWITCH");
            result.ReleaseGroup.Should().Be("Scene");
        }
    }
}
