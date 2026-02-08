using Newtonsoft.Json;

namespace Kagarr.Core.Indexers.Torznab
{
    public class TorznabSettings
    {
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }

        [JsonProperty("apiPath")]
        public string ApiPath { get; set; } = "/api";

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("categories")]
        public string Categories { get; set; } = "4000";

        [JsonProperty("minimumSeeders")]
        public int MinimumSeeders { get; set; } = 1;
    }
}
