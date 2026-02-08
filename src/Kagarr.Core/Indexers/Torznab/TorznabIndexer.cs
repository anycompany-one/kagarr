using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Kagarr.Common.Instrumentation;
using Newtonsoft.Json;
using NLog;

namespace Kagarr.Core.Indexers.Torznab
{
    public class TorznabIndexer : IIndexer
    {
        private readonly Logger _logger;
        private readonly TorznabSettings _settings;
        private readonly string _name;

        public TorznabIndexer(string name, TorznabSettings settings)
        {
            _name = name;
            _settings = settings;
            _logger = KagarrLogger.GetLogger(this);
        }

        public string Name => _name;
        public string Protocol => "torrent";

        public List<ReleaseInfo> Search(string searchTerm)
        {
            var baseUrl = _settings.BaseUrl.TrimEnd('/');
            var apiPath = _settings.ApiPath ?? "/api";
            var url = $"{baseUrl}{apiPath}?t=search&q={Uri.EscapeDataString(searchTerm)}&apikey={_settings.ApiKey}&cat={_settings.Categories}&extended=1";

            _logger.Info("Searching Torznab indexer '{0}': {1}", _name, searchTerm);

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = global::System.TimeSpan.FromSeconds(30);
                    var response = httpClient.GetStringAsync(url).Result;
                    return ParseTorznabResponse(response);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching Torznab indexer '{0}'", _name);
                return new List<ReleaseInfo>();
            }
        }

        private List<ReleaseInfo> ParseTorznabResponse(string xml)
        {
            var results = new List<ReleaseInfo>();

            try
            {
                var doc = XDocument.Parse(xml);
                XNamespace torznab = "http://torznab.com/schemas/2015/feed";

                var items = doc.Descendants("item");

                foreach (var item in items)
                {
                    var release = new ReleaseInfo
                    {
                        Guid = item.Element("guid")?.Value,
                        Title = item.Element("title")?.Value,
                        InfoUrl = item.Element("comments")?.Value,
                        Indexer = _name,
                        DownloadProtocol = "torrent",
                        PublishDate = ParseDate(item.Element("pubDate")?.Value)
                    };

                    // Prefer enclosure URL for torrent download
                    var enclosure = item.Element("enclosure");
                    if (enclosure != null)
                    {
                        release.DownloadUrl = enclosure.Attribute("url")?.Value;
                        if (long.TryParse(enclosure.Attribute("length")?.Value, out var encSize))
                        {
                            release.Size = encSize;
                        }
                    }

                    if (string.IsNullOrEmpty(release.DownloadUrl))
                    {
                        release.DownloadUrl = item.Element("link")?.Value;
                    }

                    // Parse torznab attributes
                    var attrs = item.Descendants(torznab + "attr").ToList();

                    var sizeAttr = attrs.FirstOrDefault(a => a.Attribute("name")?.Value == "size");
                    if (sizeAttr != null && long.TryParse(sizeAttr.Attribute("value")?.Value, out var size))
                    {
                        release.Size = size;
                    }

                    var seedersAttr = attrs.FirstOrDefault(a => a.Attribute("name")?.Value == "seeders");
                    if (seedersAttr != null && int.TryParse(seedersAttr.Attribute("value")?.Value, out var seeders))
                    {
                        release.Seeders = seeders;
                    }

                    var leechersAttr = attrs.FirstOrDefault(a => a.Attribute("name")?.Value == "peers");
                    if (leechersAttr != null && int.TryParse(leechersAttr.Attribute("value")?.Value, out var leechers))
                    {
                        release.Leechers = leechers;
                    }

                    var catAttrs = attrs.Where(a => a.Attribute("name")?.Value == "category");
                    release.Categories = catAttrs.Select(a => a.Attribute("value")?.Value).Where(v => v != null).ToList();

                    // Apply minimum seeders filter
                    if (release.Seeders < _settings.MinimumSeeders)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(release.Title))
                    {
                        results.Add(release);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to parse Torznab response from '{0}'", _name);
            }

            _logger.Debug("Found {0} releases from '{1}'", results.Count, _name);
            return results;
        }

        private static DateTime ParseDate(string dateStr)
        {
            if (DateTime.TryParse(dateStr, out var date))
            {
                return date.ToUniversalTime();
            }

            return DateTime.UtcNow;
        }

        public static TorznabIndexer FromDefinition(IndexerDefinition definition)
        {
            var settings = JsonConvert.DeserializeObject<TorznabSettings>(definition.Settings) ?? new TorznabSettings();
            return new TorznabIndexer(definition.Name, settings);
        }
    }
}
