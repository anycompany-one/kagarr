using Newtonsoft.Json;

namespace Kagarr.Core.Indexers.Newznab
{
    public class NewznabSettings
    {
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }

        [JsonProperty("apiPath")]
        public string ApiPath { get; set; } = "/api";

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("categories")]
        public string Categories { get; set; } = "4000";
    }
}
