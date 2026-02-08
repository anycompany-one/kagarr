using System;
using System.Collections.Generic;
using System.Net.Http;
using Kagarr.Core.Download;
using Kagarr.Core.Indexers;
using Kagarr.Core.Indexers.Newznab;
using Kagarr.Core.Indexers.Torznab;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Kagarr.Api.V1.Health
{
    [V1ApiController("health")]
    public class HealthController : Controller
    {
        private readonly IIndexerService _indexerService;
        private readonly IDownloadClientService _downloadClientService;

        public HealthController(IIndexerService indexerService, IDownloadClientService downloadClientService)
        {
            _indexerService = indexerService;
            _downloadClientService = downloadClientService;
        }

        [HttpPost("indexer/{id:int}")]
        public ActionResult<TestResult> TestIndexer(int id)
        {
            var definition = _indexerService.Get(id);
            return TestIndexerDefinition(definition);
        }

        [HttpPost("indexer/test")]
        public ActionResult<TestResult> TestNewIndexer([FromBody] IndexerTestRequest request)
        {
            var definition = new IndexerDefinition
            {
                Name = request.Name ?? "Test",
                Implementation = request.Implementation,
                Settings = request.Settings,
                EnableSearch = true,
                EnableRss = true
            };

            return TestIndexerDefinition(definition);
        }

        [HttpPost("downloadclient/{id:int}")]
        public ActionResult<TestResult> TestDownloadClient(int id)
        {
            var definition = _downloadClientService.Get(id);
            return TestDownloadClientDefinition(definition);
        }

        [HttpPost("downloadclient/test")]
        public ActionResult<TestResult> TestNewDownloadClient([FromBody] DownloadClientTestRequest request)
        {
            var definition = new DownloadClientDefinition
            {
                Name = request.Name ?? "Test",
                Implementation = request.Implementation,
                Settings = request.Settings,
                Protocol = request.Protocol ?? "torrent",
                Enable = true
            };

            return TestDownloadClientDefinition(definition);
        }

        [HttpPost("igdb")]
        public ActionResult<TestResult> TestIgdb()
        {
            try
            {
                var clientId = global::System.Environment.GetEnvironmentVariable("KAGARR_IGDB_CLIENT_ID");
                var clientSecret = global::System.Environment.GetEnvironmentVariable("KAGARR_IGDB_CLIENT_SECRET");

                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                {
                    return new TestResult { IsValid = false, Message = "IGDB client ID or secret not configured." };
                }

                using (var httpClient = new HttpClient())
                {
                    using (var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("client_id", clientId),
                        new KeyValuePair<string, string>("client_secret", clientSecret),
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                    }))
                    {
                        var response = httpClient.PostAsync("https://id.twitch.tv/oauth2/token", content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            return new TestResult { IsValid = true, Message = "IGDB connection successful." };
                        }

                        return new TestResult
                        {
                            IsValid = false,
                            Message = $"IGDB auth failed with status {response.StatusCode}. Check your client ID and secret."
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new TestResult { IsValid = false, Message = $"Connection failed: {ex.Message}" };
            }
        }

        [HttpPost("discord")]
        public ActionResult<TestResult> TestDiscord()
        {
            var webhookUrl = global::System.Environment.GetEnvironmentVariable("KAGARR_DISCORD_WEBHOOK");

            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                return new TestResult { IsValid = false, Message = "Discord webhook URL not configured." };
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Send a test embed
                    var payload = new
                    {
                        embeds = new[]
                        {
                            new
                            {
                                title = "Kagarr Test",
                                description = "Connection test successful.",
                                color = 3447003,
                                footer = new { text = "Kagarr" }
                            }
                        }
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    using (var content = new StringContent(json, global::System.Text.Encoding.UTF8, "application/json"))
                    {
                        var response = httpClient.PostAsync(webhookUrl, content).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            return new TestResult { IsValid = true, Message = "Discord webhook test sent successfully." };
                        }

                        return new TestResult
                        {
                            IsValid = false,
                            Message = $"Discord webhook failed with status {response.StatusCode}."
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new TestResult { IsValid = false, Message = $"Connection failed: {ex.Message}" };
            }
        }

        private static TestResult TestIndexerDefinition(IndexerDefinition definition)
        {
            try
            {
                IIndexer indexer = null;

                switch (definition.Implementation?.ToLowerInvariant())
                {
                    case "newznab":
                        indexer = NewznabIndexer.FromDefinition(definition);
                        break;
                    case "torznab":
                        indexer = TorznabIndexer.FromDefinition(definition);
                        break;
                    default:
                        return new TestResult
                        {
                            IsValid = false,
                            Message = $"Unknown implementation: {definition.Implementation}"
                        };
                }

                // Try a search with an empty query to test connectivity
                indexer.Search("test");

                return new TestResult { IsValid = true, Message = $"Successfully connected to {definition.Name}." };
            }
            catch (Exception ex)
            {
                return new TestResult { IsValid = false, Message = $"Connection failed: {ex.Message}" };
            }
        }

        private static TestResult TestDownloadClientDefinition(DownloadClientDefinition definition)
        {
            try
            {
                IDownloadClient client = null;

                switch (definition.Implementation?.ToLowerInvariant())
                {
                    case "qbittorrent":
                        client = Kagarr.Core.Download.Clients.QBittorrent.QBittorrentClient.FromDefinition(definition);
                        break;
                    case "sabnzbd":
                        client = Kagarr.Core.Download.Clients.Sabnzbd.SabnzbdClient.FromDefinition(definition);
                        break;
                    default:
                        return new TestResult
                        {
                            IsValid = false,
                            Message = $"Unknown implementation: {definition.Implementation}"
                        };
                }

                // Try to get queue items to verify connectivity
                client.GetItems();

                return new TestResult { IsValid = true, Message = $"Successfully connected to {definition.Name}." };
            }
            catch (Exception ex)
            {
                return new TestResult { IsValid = false, Message = $"Connection failed: {ex.Message}" };
            }
        }
    }

    public class TestResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
    }

    public class IndexerTestRequest
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string Settings { get; set; }
    }

    public class DownloadClientTestRequest
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string Settings { get; set; }
        public string Protocol { get; set; }
    }
}
