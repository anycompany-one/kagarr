using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kagarr.Core.MetadataSource.Igdb
{
    public class IgdbGameResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("first_release_date")]
        public long? FirstReleaseDate { get; set; }

        [JsonProperty("cover")]
        public IgdbCoverResource Cover { get; set; }

        [JsonProperty("genres")]
        public List<IgdbGenreResource> Genres { get; set; }

        [JsonProperty("platforms")]
        public List<IgdbPlatformResource> Platforms { get; set; }

        [JsonProperty("involved_companies")]
        public List<IgdbInvolvedCompanyResource> InvolvedCompanies { get; set; }

        [JsonProperty("screenshots")]
        public List<IgdbScreenshotResource> Screenshots { get; set; }
    }

    public class IgdbCoverResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }
    }

    public class IgdbGenreResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class IgdbPlatformResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("abbreviation")]
        public string Abbreviation { get; set; }
    }

    public class IgdbInvolvedCompanyResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("company")]
        public IgdbCompanyResource Company { get; set; }

        [JsonProperty("developer")]
        public bool Developer { get; set; }

        [JsonProperty("publisher")]
        public bool Publisher { get; set; }
    }

    public class IgdbCompanyResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class IgdbScreenshotResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("image_id")]
        public string ImageId { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }
    }

    public class IgdbAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
