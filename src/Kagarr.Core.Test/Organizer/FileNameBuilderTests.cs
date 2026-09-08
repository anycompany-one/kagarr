using FluentAssertions;
using Kagarr.Core.Games;
using Kagarr.Core.Organizer;
using Kagarr.Core.Platforms;
using NUnit.Framework;

namespace Kagarr.Core.Test.Organizer
{
    [TestFixture]
    public class FileNameBuilderTests
    {
        private Game _game;

        [SetUp]
        public void Setup()
        {
            _game = new Game
            {
                Id = 1,
                Title = "Test Game",
                Year = 2020,
                Platform = GamePlatform.PC
            };
        }

        [Test]
        public void BuildGameFileName_should_use_standard_name()
        {
            var result = FileNameBuilder.BuildGameFileName(_game, "/downloads/whatever.iso");

            result.Should().Be("Test Game (2020) [PC].iso");
        }

        [Test]
        public void BuildGameFileName_with_original_name_should_include_original_file_name()
        {
            var result = FileNameBuilder.BuildGameFileName(_game, "/downloads/disc1.iso", true);

            result.Should().Be("Test Game (2020) [PC] - disc1.iso");
        }

        [TestCase("..", "Unknown Game")]
        [TestCase(".", "Unknown Game")]
        [TestCase("...", "Unknown Game")]
        [TestCase("", "")]
        [TestCase("   ", "Unknown Game")]
        [TestCase(". .", "Unknown Game")]
        public void CleanFileName_should_replace_dangerous_or_empty_results_with_fallback(string input, string expected)
        {
            FileNameBuilder.CleanFileName(input).Should().Be(expected);
        }

        [Test]
        public void CleanFileName_should_trim_trailing_dots_and_spaces()
        {
            FileNameBuilder.CleanFileName("Game Title... ").Should().Be("Game Title");
        }

        [TestCase("a<b", "a_b")]
        [TestCase("a>b", "a_b")]
        [TestCase("a:b", "a_b")]
        [TestCase("a\"b", "a_b")]
        [TestCase("a/b", "a_b")]
        [TestCase("a\\b", "a_b")]
        [TestCase("a|b", "a_b")]
        [TestCase("a?b", "a_b")]
        [TestCase("a*b", "a_b")]
        [TestCase("a\tb", "a_b")]
        [TestCase("a\nb", "a_b")]
        [TestCase("a\0b", "a_b")]
        public void CleanFileName_should_replace_reserved_characters_on_all_platforms(string input, string expected)
        {
            FileNameBuilder.CleanFileName(input).Should().Be(expected);
        }

        [Test]
        public void CleanFileName_should_not_allow_path_traversal_segments()
        {
            var root = "/library";
            var cleaned = FileNameBuilder.CleanFileName("..");
            var combined = global::System.IO.Path.GetFullPath(global::System.IO.Path.Combine(root, cleaned));

            combined.Should().StartWith(root + "/");
        }

        [Test]
        public void BuildGameFolder_with_traversal_title_should_stay_inside_root()
        {
            _game.Title = "..";

            var result = FileNameBuilder.BuildGameFolder(_game);

            result.Should().Be("Unknown Game (2020) [PC]");
        }

        [Test]
        public void BuildGameFileName_with_original_name_should_produce_unique_names_for_different_files()
        {
            var first = FileNameBuilder.BuildGameFileName(_game, "/downloads/disc1.iso", true);
            var second = FileNameBuilder.BuildGameFileName(_game, "/downloads/disc2.iso", true);

            first.Should().NotBe(second);
        }
    }
}
