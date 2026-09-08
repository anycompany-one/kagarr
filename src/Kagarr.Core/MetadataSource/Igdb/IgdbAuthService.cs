using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Newtonsoft.Json;
using NLog;

namespace Kagarr.Core.MetadataSource.Igdb
{
    public interface IIgdbAuthService
    {
        Task<string> GetAccessTokenAsync();
        string GetClientId();
    }

    public sealed class IgdbAuthService : IIgdbAuthService, IDisposable
    {
        private readonly Logger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private string _accessToken;
        private DateTime _tokenExpiry;

        public IgdbAuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

        public async Task<string> GetAccessTokenAsync()
        {
            await _lock.WaitAsync();

            try
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

                var httpClient = _httpClientFactory.CreateClient("igdb");

                using (var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                }))
                {
                    using (var response = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", content))
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.Error("Failed to obtain IGDB access token. Status: {0}, Body: {1}", response.StatusCode, responseBody);
                            throw new HttpRequestException($"Failed to obtain IGDB access token. Status: {response.StatusCode}");
                        }

                        var authResponse = JsonConvert.DeserializeObject<IgdbAuthResponse>(responseBody);

                        if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.AccessToken))
                        {
                            _logger.Error("IGDB token endpoint returned an unexpected body: {0}", responseBody);
                            throw new HttpRequestException("IGDB token endpoint returned an unexpected response body (no access token)");
                        }

                        _accessToken = authResponse.AccessToken;

                        // Expire 5 minutes early to avoid edge cases, but never less than 30 seconds from now
                        var lifetimeSeconds = Math.Max(authResponse.ExpiresIn - 300, 30);
                        _tokenExpiry = DateTime.UtcNow.AddSeconds(lifetimeSeconds);

                        _logger.Info("Successfully obtained IGDB access token, expires in {0} seconds", authResponse.ExpiresIn);

                        return _accessToken;
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Dispose()
        {
            _lock.Dispose();
        }
    }
}
