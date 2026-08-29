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

        [Test]
        public void BuildGameFileName_with_original_name_should_produce_unique_names_for_different_files()
        {
            var first = FileNameBuilder.BuildGameFileName(_game, "/downloads/disc1.iso", true);
            var second = FileNameBuilder.BuildGameFileName(_game, "/downloads/disc2.iso", true);

            first.Should().NotBe(second);
        }
    }
}
