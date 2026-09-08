using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Deals;
using Kagarr.Core.Games;
using Kagarr.Core.Wishlist;
using Newtonsoft.Json;
using NLog;

namespace Kagarr.Core.Notifications
{
    public class DiscordWebhookService : INotificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Logger _logger;

        public DiscordWebhookService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _logger = KagarrLogger.GetLogger(this);
        }

        public Task OnGameAddedAsync(Game game)
        {
            return SendNotificationAsync(
                "Game Added",
                $"**{game.Title}** ({game.Year}) has been added to the library.",
                3447003,
                game.Images?.Find(i => i.CoverType == MediaCoverTypes.Cover)?.RemoteUrl);
        }

        public Task OnGameFileImportedAsync(Game game, string filePath)
        {
            var fileName = global::System.IO.Path.GetFileName(filePath);
            return SendNotificationAsync(
                "Game Imported",
                $"**{game.Title}** has been imported.\nFile: `{fileName}`",
                2278750,
                game.Images?.Find(i => i.CoverType == MediaCoverTypes.Cover)?.RemoteUrl);
        }

        public Task OnGameGrabbedAsync(Game game, string releaseTitle)
        {
            return SendNotificationAsync(
                "Game Grabbed",
                $"**{game.Title}** release grabbed.\nRelease: `{releaseTitle}`",
                15844367,
                game.Images?.Find(i => i.CoverType == MediaCoverTypes.Cover)?.RemoteUrl);
        }

        public Task OnDealFoundAsync(WishlistItem item, GameDeal deal)
        {
            var priceText = deal.IsFree
                ? "**FREE**"
                : string.Format(CultureInfo.InvariantCulture, "**${0:F2}**", deal.CurrentPrice);

            var description = $"**{item.Title}** is on sale at **{deal.Store}**!\n" +
                              $"Price: {priceText}";

            if (deal.DiscountPercent > 0)
            {
                description += string.Format(
                    CultureInfo.InvariantCulture,
                    " ({0}% off, was ${1:F2})",
                    deal.DiscountPercent,
                    deal.RegularPrice);
            }

            if (!string.IsNullOrWhiteSpace(deal.DealUrl))
            {
                description += $"\n[View Deal]({deal.DealUrl})";
            }

            var coverUrl = item.Images?.Find(i => i.CoverType == MediaCoverTypes.Cover)?.RemoteUrl;

            // Gold color for deal alerts
            return SendNotificationAsync("Deal Alert", description, 16766720, coverUrl);
        }

        private async Task SendNotificationAsync(string title, string description, int color, string coverUrl)
        {
            var webhookUrl = global::System.Environment.GetEnvironmentVariable("KAGARR_DISCORD_WEBHOOK");
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                return;
            }

            try
            {
                var embed = new
                {
                    title,
                    description,
                    color,
                    thumbnail = coverUrl != null ? new { url = coverUrl } : null,
                    footer = new { text = "Kagarr" },
                    timestamp = global::System.DateTime.UtcNow.ToString("o")
                };

                var payload = new
                {
                    embeds = new[] { embed }
                };

                var json = JsonConvert.SerializeObject(payload);

                var httpClient = _httpClientFactory.CreateClient("discord");

                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    using (var response = await httpClient.PostAsync(webhookUrl, content))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.Warn("Discord webhook failed. Status: {0}", response.StatusCode);
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Warn(ex, "Failed to send Discord notification");
            }
        }
    }
}
