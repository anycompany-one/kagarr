using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Http;
using Newtonsoft.Json.Linq;
using NLog;

namespace Kagarr.Core.Deals.Sources
{
    public class SteamDealSource : IDealSource
    {
        private static readonly TimeSpan SteamRateInterval = TimeSpan.FromMilliseconds(1500);

        private readonly IRateLimitService _rateLimitService;
        private readonly Logger _logger;

        public SteamDealSource(IRateLimitService rateLimitService)
        {
            _rateLimitService = rateLimitService;
            _logger = KagarrLogger.GetLogger(this);
        }

        public string Name => "Steam";

        public List<GameDeal> GetDeals(string gameTitle, int? steamAppId)
        {
            var deals = new List<GameDeal>();

            if (!steamAppId.HasValue || steamAppId.Value <= 0)
            {
                return deals;
            }

            try
            {
                _rateLimitService.WaitAndPulse("steam", SteamRateInterval);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Kagarr/1.0");

                    var url = $"https://store.steampowered.com/api/appdetails?appids={steamAppId.Value}&cc=us&filters=price_overview";
                    var response = client.GetStringAsync(url).Result;
                    var json = JObject.Parse(response);

                    var appData = json[steamAppId.Value.ToString(CultureInfo.InvariantCulture)];
                    if (appData?["success"]?.Value<bool>() != true)
                    {
                        return deals;
                    }

                    var priceOverview = appData["data"]?["price_overview"];
                    if (priceOverview == null)
                    {
                        // Game might be free
                        var isFree = appData["data"]?["is_free"]?.Value<bool>() ?? false;
                        if (isFree)
                        {
                            deals.Add(new GameDeal
                            {
                                Store = "Steam",
                                Title = gameTitle,
                                CurrentPrice = 0m,
                                RegularPrice = 0m,
                                DiscountPercent = 0,
                                CurrencyCode = "USD",
                                DealUrl = $"https://store.steampowered.com/app/{steamAppId.Value}",
                                IsFree = true
                            });
                        }

                        return deals;
                    }

                    var currentCents = priceOverview["final"]?.Value<int>() ?? 0;
                    var regularCents = priceOverview["initial"]?.Value<int>() ?? 0;
                    var discount = priceOverview["discount_percent"]?.Value<int>() ?? 0;
                    var currency = priceOverview["currency"]?.Value<string>() ?? "USD";

                    deals.Add(new GameDeal
                    {
                        Store = "Steam",
                        Title = gameTitle,
                        CurrentPrice = currentCents / 100m,
                        RegularPrice = regularCents / 100m,
                        DiscountPercent = discount,
                        CurrencyCode = currency,
                        DealUrl = $"https://store.steampowered.com/app/{steamAppId.Value}",
                        IsFree = currentCents == 0
                    });
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Warn(ex, "Failed to fetch Steam price for app {0}", steamAppId.Value);
            }

            return deals;
        }
    }
}
