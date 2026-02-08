using Newtonsoft.Json;

namespace Kagarr.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdSettings
    {
        [JsonProperty("host")]
        public string Host { get; set; } = "localhost";

        [JsonProperty("port")]
        public int Port { get; set; } = 8080;

        [JsonProperty("useSsl")]
        public bool UseSsl { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; } = "kagarr";

        public string GetBaseUrl()
        {
            var scheme = UseSsl ? "https" : "http";
            return $"{scheme}://{Host}:{Port}";
        }
    }
}
