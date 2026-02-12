using System.Collections.Generic;
using FluentAssertions;
using Kagarr.Core.Indexers;
using Moq;
using NUnit.Framework;

namespace Kagarr.Core.Test.Indexer
{
    [TestFixture]
    public class IndexerServiceTests
    {
        private Mock<IIndexerRepository> _indexerRepo;
        private IndexerService _service;

        [SetUp]
        public void Setup()
        {
            _indexerRepo = new Mock<IIndexerRepository>();
            _service = new IndexerService(_indexerRepo.Object);
        }

        [Test]
        public void SearchAllIndexers_with_no_enabled_indexers_should_return_empty()
        {
            _indexerRepo.Setup(r => r.All()).Returns(new List<IndexerDefinition>
            {
                new IndexerDefinition
                {
                    Id = 1,
                    Name = "Disabled Indexer",
                    Implementation = "newznab",
                    EnableSearch = false
                }
            });

            var result = _service.SearchAllIndexers("test game");

            result.Should().BeEmpty();
        }

        [Test]
        public void SearchAllIndexers_with_unknown_implementation_should_skip()
        {
            _indexerRepo.Setup(r => r.All()).Returns(new List<IndexerDefinition>
            {
                new IndexerDefinition
                {
                    Id = 1,
                    Name = "Unknown Indexer",
                    Implementation = "invalid_type",
                    EnableSearch = true
                }
            });

            var result = _service.SearchAllIndexers("test game");

            result.Should().BeEmpty();
        }

        [Test]
        public void All_should_return_all_definitions()
        {
            var definitions = new List<IndexerDefinition>
            {
                new IndexerDefinition { Id = 1, Name = "Indexer A" },
                new IndexerDefinition { Id = 2, Name = "Indexer B" }
            };

            _indexerRepo.Setup(r => r.All()).Returns(definitions);

            var result = _service.All();

            result.Should().HaveCount(2);
            result[0].Name.Should().Be("Indexer A");
        }
    }
}
