using System;
using System.Collections.Generic;
using System.Net.Http;
using Kagarr.Common.Instrumentation;
using Newtonsoft.Json;
using NLog;

namespace Kagarr.Core.MetadataSource.Igdb
{
    public interface IIgdbAuthService
    {
        string GetAccessToken();
        string GetClientId();
    }

    public class IgdbAuthService : IIgdbAuthService
    {
        private readonly Logger _logger;
        private readonly object _lock = new object();

        private string _accessToken;
        private DateTime _tokenExpiry;

        public IgdbAuthService()
        {
            _logger = KagarrLogger.GetLogger(this);
            _tokenExpiry = DateTime.MinValue;
        }

        public string GetClientId()
        {
            var clientId = global::System.Environment.GetEnvironmentVariable("KAGARR_IGDB_CLIENT_ID");

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new InvalidOperationException("KAGARR_IGDB_CLIENT_ID environment variable is not set. Configure your Twitch/IGDB client credentials.");
            }

            return clientId;
        }

        public string GetAccessToken()
        {
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
                {
                    return _accessToken;
                }

                _logger.Info("Requesting new IGDB access token via Twitch OAuth");

                var clientId = GetClientId();
                var clientSecret = global::System.Environment.GetEnvironmentVariable("KAGARR_IGDB_CLIENT_SECRET");

                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    throw new InvalidOperationException("KAGARR_IGDB_CLIENT_SECRET environment variable is not set. Configure your Twitch/IGDB client credentials.");
                }

                using (var httpClient = new HttpClient())
                {
                    using (var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("client_id", clientId),
                        new KeyValuePair<string, string>("client_secret", clientSecret),
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                    }))
                    {
                        var response = httpClient.PostAsync("https://id.twitch.tv/oauth2/token", content).Result;
                        var responseBody = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.Error("Failed to obtain IGDB access token. Status: {0}, Body: {1}", response.StatusCode, responseBody);
                            throw new HttpRequestException($"Failed to obtain IGDB access token. Status: {response.StatusCode}");
                        }

                        var authResponse = JsonConvert.DeserializeObject<IgdbAuthResponse>(responseBody);

                        _accessToken = authResponse.AccessToken;

                        // Expire 5 minutes early to avoid edge cases
                        _tokenExpiry = DateTime.UtcNow.AddSeconds(authResponse.ExpiresIn - 300);

                        _logger.Info("Successfully obtained IGDB access token, expires in {0} seconds", authResponse.ExpiresIn);

                        return _accessToken;
                    }
                }
            }
        }
    }
}
