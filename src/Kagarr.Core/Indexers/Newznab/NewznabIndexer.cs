using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Kagarr.Common.Instrumentation;
using Newtonsoft.Json;
using NLog;

namespace Kagarr.Core.Indexers.Newznab
{
    public class NewznabIndexer : IIndexer
    {
        private readonly Logger _logger;
        private readonly NewznabSettings _settings;
        private readonly string _name;

        public NewznabIndexer(string name, NewznabSettings settings)
        {
            _name = name;
            _settings = settings;
            _logger = KagarrLogger.GetLogger(this);
        }

        public string Name => _name;
        public string Protocol => "usenet";

        public List<ReleaseInfo> Search(string searchTerm)
        {
            var baseUrl = _settings.BaseUrl.TrimEnd('/');
            var apiPath = _settings.ApiPath ?? "/api";
            var url = $"{baseUrl}{apiPath}?t=search&q={Uri.EscapeDataString(searchTerm)}&apikey={_settings.ApiKey}&cat={_settings.Categories}&extended=1";

            _logger.Info("Searching Newznab indexer '{0}': {1}", _name, searchTerm);

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = global::System.TimeSpan.FromSeconds(30);
                    var response = httpClient.GetStringAsync(url).Result;
                    return ParseNewznabResponse(response);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error searching Newznab indexer '{0}'", _name);
                return new List<ReleaseInfo>();
            }
        }

        private List<ReleaseInfo> ParseNewznabResponse(string xml)
        {
            var results = new List<ReleaseInfo>();

            try
            {
                var doc = XDocument.Parse(xml);
                XNamespace newznab = "http://www.newznab.com/DTD/2010/feeds/attributes/";

                var items = doc.Descendants("item");

                foreach (var item in items)
                {
                    var release = new ReleaseInfo
                    {
                        Guid = item.Element("guid")?.Value,
                        Title = item.Element("title")?.Value,
                        DownloadUrl = item.Element("link")?.Value,
                        InfoUrl = item.Element("comments")?.Value,
                        Indexer = _name,
                        DownloadProtocol = "usenet",
                        PublishDate = ParseDate(item.Element("pubDate")?.Value)
                    };

                    // Parse size from newznab:attr
                    var sizeAttr = item.Descendants(newznab + "attr")
                        .FirstOrDefault(a => a.Attribute("name")?.Value == "size");
                    if (sizeAttr != null && long.TryParse(sizeAttr.Attribute("value")?.Value, out var size))
                    {
                        release.Size = size;
                    }

                    // Parse categories
                    var catAttrs = item.Descendants(newznab + "attr")
                        .Where(a => a.Attribute("name")?.Value == "category");
                    release.Categories = catAttrs.Select(a => a.Attribute("value")?.Value).Where(v => v != null).ToList();

                    if (!string.IsNullOrEmpty(release.Title))
                    {
                        results.Add(release);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to parse Newznab response from '{0}'", _name);
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

        public static NewznabIndexer FromDefinition(IndexerDefinition definition)
        {
            var settings = JsonConvert.DeserializeObject<NewznabSettings>(definition.Settings) ?? new NewznabSettings();
            return new NewznabIndexer(definition.Name, settings);
        }
    }
}
