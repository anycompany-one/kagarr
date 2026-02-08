using System.Net.Http;
using System.Text;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Games;
using Newtonsoft.Json;
using NLog;

namespace Kagarr.Core.Notifications
{
    public class DiscordWebhookService : INotificationService
    {
        private readonly Logger _logger;

        public DiscordWebhookService()
        {
            _logger = KagarrLogger.GetLogger(this);
        }

        public void OnGameAdded(Game game)
        {
            SendNotification(
                "Game Added",
                $"**{game.Title}** ({game.Year}) has been added to the library.",
                3447003,
                game);
        }

        public void OnGameFileImported(Game game, string filePath)
        {
            var fileName = global::System.IO.Path.GetFileName(filePath);
            SendNotification(
                "Game Imported",
                $"**{game.Title}** has been imported.\nFile: `{fileName}`",
                2278750,
                game);
        }

        public void OnGameGrabbed(Game game, string releaseTitle)
        {
            SendNotification(
                "Game Grabbed",
                $"**{game.Title}** release grabbed.\nRelease: `{releaseTitle}`",
                15844367,
                game);
        }

        private void SendNotification(string title, string description, int color, Game game)
        {
            var webhookUrl = global::System.Environment.GetEnvironmentVariable("KAGARR_DISCORD_WEBHOOK");
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                return;
            }

            try
            {
                var coverUrl = game.Images?.Find(i => i.CoverType == MediaCoverTypes.Cover)?.RemoteUrl;

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

                using (var httpClient = new HttpClient())
                {
                    using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        var response = httpClient.PostAsync(webhookUrl, content).Result;

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
