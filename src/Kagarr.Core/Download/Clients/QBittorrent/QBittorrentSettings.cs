using Newtonsoft.Json;

namespace Kagarr.Core.Download.Clients.QBittorrent
{
    public class QBittorrentSettings
    {
        [JsonProperty("host")]
        public string Host { get; set; } = "localhost";

        [JsonProperty("port")]
        public int Port { get; set; } = 8080;

        [JsonProperty("useSsl")]
        public bool UseSsl { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; } = "kagarr";

        public string GetBaseUrl()
        {
            var scheme = UseSsl ? "https" : "http";
            return $"{scheme}://{Host}:{Port}";
        }
    }
}
