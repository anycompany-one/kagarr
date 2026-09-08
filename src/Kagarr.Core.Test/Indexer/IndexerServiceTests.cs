using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
        private Mock<IHttpClientFactory> _httpClientFactory;
        private IndexerService _service;

        [SetUp]
        public void Setup()
        {
            _indexerRepo = new Mock<IIndexerRepository>();
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _httpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(() => new HttpClient());
            _service = new IndexerService(_indexerRepo.Object, _httpClientFactory.Object);
        }

        [Test]
        public async Task SearchAllIndexers_with_no_enabled_indexers_should_return_empty()
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

            var result = await _service.SearchAllIndexersAsync("test game");

            result.Should().BeEmpty();
        }

        [Test]
        public async Task SearchAllIndexers_with_unknown_implementation_should_skip()
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

            var result = await _service.SearchAllIndexersAsync("test game");

            result.Should().BeEmpty();
        }

        [Test]
        public async Task SearchAllIndexers_should_not_fail_when_one_indexer_throws()
        {
            _indexerRepo.Setup(r => r.All()).Returns(new List<IndexerDefinition>
            {
                new IndexerDefinition { Id = 1, Name = "Broken", Implementation = "torznab", EnableSearch = true },
                new IndexerDefinition { Id = 2, Name = "Working", Implementation = "torznab", EnableSearch = true }
            });

            var broken = new Mock<IIndexer>();
            broken.Setup(i => i.SearchAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("indexer down"));

            var working = new Mock<IIndexer>();
            working.Setup(i => i.SearchAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ReleaseInfo>
                {
                    new ReleaseInfo { Title = "Game.Release-GROUP", Seeders = 10 }
                });

            var service = new TestableIndexerService(_indexerRepo.Object, _httpClientFactory.Object)
            {
                IndexerFactory = definition => definition.Id == 1 ? broken.Object : working.Object
            };

            var result = await service.SearchAllIndexersAsync("test game");

            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Game.Release-GROUP");
            result[0].IndexerId.Should().Be(2);
        }

        [Test]
        public async Task SearchAllIndexers_should_merge_and_sort_results_from_all_indexers()
        {
            _indexerRepo.Setup(r => r.All()).Returns(new List<IndexerDefinition>
            {
                new IndexerDefinition { Id = 1, Name = "A", Implementation = "torznab", EnableSearch = true },
                new IndexerDefinition { Id = 2, Name = "B", Implementation = "torznab", EnableSearch = true }
            });

            var indexerA = new Mock<IIndexer>();
            indexerA.Setup(i => i.SearchAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ReleaseInfo> { new ReleaseInfo { Title = "Low Seeders", Seeders = 1 } });

            var indexerB = new Mock<IIndexer>();
            indexerB.Setup(i => i.SearchAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<ReleaseInfo> { new ReleaseInfo { Title = "High Seeders", Seeders = 100 } });

            var service = new TestableIndexerService(_indexerRepo.Object, _httpClientFactory.Object)
            {
                IndexerFactory = definition => definition.Id == 1 ? indexerA.Object : indexerB.Object
            };

            var result = await service.SearchAllIndexersAsync("test game");

            result.Should().HaveCount(2);
            result[0].Title.Should().Be("High Seeders");
            result[1].Title.Should().Be("Low Seeders");
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

        private sealed class TestableIndexerService : IndexerService
        {
            public TestableIndexerService(IIndexerRepository indexerRepository, IHttpClientFactory httpClientFactory)
                : base(indexerRepository, httpClientFactory)
            {
            }

            public Func<IndexerDefinition, IIndexer> IndexerFactory { get; init; }

            protected internal override IIndexer CreateIndexer(IndexerDefinition definition)
            {
                return IndexerFactory(definition);
            }
        }
    }
}
