using System;
using System.Collections.Generic;
using System.Net.Http;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Http;
using Newtonsoft.Json.Linq;
using NLog;

namespace Kagarr.Core.Deals.Sources
{
    public class ItadDealSource : IDealSource
    {
        private static readonly TimeSpan ItadRateInterval = TimeSpan.FromMilliseconds(250);

        private readonly IRateLimitService _rateLimitService;
        private readonly Logger _logger;

        public ItadDealSource(IRateLimitService rateLimitService)
        {
            _rateLimitService = rateLimitService;
            _logger = KagarrLogger.GetLogger(this);
        }

        public string Name => "IsThereAnyDeal";

        public List<GameDeal> GetDeals(string gameTitle, int? steamAppId)
        {
            var deals = new List<GameDeal>();

            var apiKey = global::System.Environment.GetEnvironmentVariable("KAGARR_ITAD_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return deals;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Kagarr/1.0");

                    // First find the game's ITAD plain (slug)
                    _rateLimitService.WaitAndPulse("itad", ItadRateInterval);
                    var encodedTitle = Uri.EscapeDataString(gameTitle);
                    var searchUrl = $"https://api.isthereanydeal.com/v02/search/search/?key={apiKey}&q={encodedTitle}&limit=1";
                    var searchResponse = client.GetStringAsync(searchUrl).Result;
                    var searchJson = JObject.Parse(searchResponse);
                    var resultsList = searchJson["data"]?["results"] as JArray;

                    if (resultsList == null || resultsList.Count == 0)
                    {
                        return deals;
                    }

                    var plain = resultsList[0]?["plain"]?.Value<string>();
                    if (string.IsNullOrWhiteSpace(plain))
                    {
                        return deals;
                    }

                    // Get current prices
                    _rateLimitService.WaitAndPulse("itad", ItadRateInterval);
                    var priceUrl = $"https://api.isthereanydeal.com/v01/game/prices/?key={apiKey}&plains={plain}&country=US";
                    var priceResponse = client.GetStringAsync(priceUrl).Result;
                    var priceJson = JObject.Parse(priceResponse);
                    var priceData = priceJson["data"]?[plain]?["list"] as JArray;

                    if (priceData == null)
                    {
                        return deals;
                    }

                    foreach (var listing in priceData)
                    {
                        var priceNew = listing["price_new"]?.Value<decimal>() ?? 0;
                        var priceOld = listing["price_old"]?.Value<decimal>() ?? 0;
                        var priceCut = listing["price_cut"]?.Value<int>() ?? 0;
                        var shop = listing["shop"]?["name"]?.Value<string>() ?? "Unknown";
                        var dealUrl = listing["url"]?.Value<string>() ?? string.Empty;

                        deals.Add(new GameDeal
                        {
                            Store = shop,
                            Title = gameTitle,
                            CurrentPrice = priceNew,
                            RegularPrice = priceOld,
                            DiscountPercent = priceCut,
                            CurrencyCode = "USD",
                            DealUrl = dealUrl,
                            IsFree = priceNew == 0
                        });
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Warn(ex, "Failed to fetch ITAD deals for '{0}'", gameTitle);
            }

            return deals;
        }
    }
}
