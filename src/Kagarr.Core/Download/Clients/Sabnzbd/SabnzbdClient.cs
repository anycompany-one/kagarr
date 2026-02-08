using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Indexers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Kagarr.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdClient : IDownloadClient
    {
        private readonly Logger _logger;
        private readonly SabnzbdSettings _settings;
        private readonly string _name;

        public SabnzbdClient(string name, SabnzbdSettings settings)
        {
            _name = name;
            _settings = settings;
            _logger = KagarrLogger.GetLogger(this);
        }

        public string Name => _name;
        public string Protocol => "usenet";

        public string Download(ReleaseInfo release)
        {
            _logger.Info("Sending '{0}' to SABnzbd", release.Title);

            var baseUrl = _settings.GetBaseUrl();
            var url = $"{baseUrl}/api?mode=addurl&name={Uri.EscapeDataString(release.DownloadUrl)}&cat={Uri.EscapeDataString(_settings.Category)}&apikey={_settings.ApiKey}&output=json";

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = global::System.TimeSpan.FromSeconds(30);
                var response = httpClient.GetStringAsync(url).Result;
                var json = JObject.Parse(response);

                if (json["status"]?.Value<bool>() != true)
                {
                    var error = json["error"]?.ToString() ?? "Unknown error";
                    _logger.Error("SABnzbd rejected NZB: {0}", error);
                    throw new HttpRequestException($"SABnzbd error: {error}");
                }

                var nzoIds = json["nzo_ids"] as JArray;
                var nzoId = nzoIds?.FirstOrDefault()?.ToString() ?? release.Guid;

                _logger.Info("Successfully sent '{0}' to SABnzbd (nzo_id: {1})", release.Title, nzoId);
                return nzoId;
            }
        }

        public List<DownloadClientItem> GetItems()
        {
            var items = new List<DownloadClientItem>();
            var baseUrl = _settings.GetBaseUrl();

            // Get queue items
            var queueUrl = $"{baseUrl}/api?mode=queue&cat={Uri.EscapeDataString(_settings.Category)}&apikey={_settings.ApiKey}&output=json";

            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = global::System.TimeSpan.FromSeconds(30);

                try
                {
                    var queueResponse = httpClient.GetStringAsync(queueUrl).Result;
                    var queueJson = JObject.Parse(queueResponse);
                    var slots = queueJson["queue"]?["slots"] as JArray ?? new JArray();

                    foreach (var slot in slots)
                    {
                        items.Add(new DownloadClientItem
                        {
                            DownloadId = slot["nzo_id"]?.ToString(),
                            Title = slot["filename"]?.ToString(),
                            TotalSize = ParseSabnzbdSize(slot["mb"]?.ToString()),
                            RemainingSize = ParseSabnzbdSize(slot["mbleft"]?.ToString()),
                            Category = slot["cat"]?.ToString(),
                            Status = MapQueueStatus(slot["status"]?.ToString()),
                            DownloadClientName = _name
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to get SABnzbd queue");
                }

                // Get history items
                var historyUrl = $"{baseUrl}/api?mode=history&cat={Uri.EscapeDataString(_settings.Category)}&apikey={_settings.ApiKey}&output=json&limit=30";

                try
                {
                    var historyResponse = httpClient.GetStringAsync(historyUrl).Result;
                    var historyJson = JObject.Parse(historyResponse);
                    var historySlots = historyJson["history"]?["slots"] as JArray ?? new JArray();

                    foreach (var slot in historySlots)
                    {
                        items.Add(new DownloadClientItem
                        {
                            DownloadId = slot["nzo_id"]?.ToString(),
                            Title = slot["name"]?.ToString(),
                            TotalSize = slot["bytes"]?.Value<long>() ?? 0,
                            RemainingSize = 0,
                            OutputPath = slot["storage"]?.ToString(),
                            Category = slot["category"]?.ToString(),
                            Status = MapHistoryStatus(slot["status"]?.ToString()),
                            DownloadClientName = _name
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to get SABnzbd history");
                }
            }

            return items;
        }

        private static long ParseSabnzbdSize(string mbString)
        {
            if (double.TryParse(mbString, global::System.Globalization.NumberStyles.Float, global::System.Globalization.CultureInfo.InvariantCulture, out var mb))
            {
                return (long)(mb * 1024 * 1024);
            }

            return 0;
        }

        private static DownloadItemStatus MapQueueStatus(string status)
        {
            switch (status?.ToLowerInvariant())
            {
                case "downloading":
                    return DownloadItemStatus.Downloading;
                case "paused":
                    return DownloadItemStatus.Paused;
                default:
                    return DownloadItemStatus.Queued;
            }
        }

        private static DownloadItemStatus MapHistoryStatus(string status)
        {
            switch (status?.ToLowerInvariant())
            {
                case "completed":
                    return DownloadItemStatus.Completed;
                case "failed":
                    return DownloadItemStatus.Failed;
                default:
                    return DownloadItemStatus.Completed;
            }
        }

        public static SabnzbdClient FromDefinition(DownloadClientDefinition definition)
        {
            var settings = JsonConvert.DeserializeObject<SabnzbdSettings>(definition.Settings) ?? new SabnzbdSettings();
            return new SabnzbdClient(definition.Name, settings);
        }
    }
}
