using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Microsoft.AspNetCore.Http;
using NLog;

namespace Kagarr.Host.Authentication
{
    public class ApiKeyMiddleware
    {
        private const string ApiKeyHeader = "X-Api-Key";
        private const string ApiKeyQuery = "apikey";

        private readonly RequestDelegate _next;
        private readonly Logger _logger;
        private readonly string _apiKey;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
            _logger = KagarrLogger.GetLogger(typeof(ApiKeyMiddleware));
            _apiKey = global::System.Environment.GetEnvironmentVariable("KAGARR_API_KEY");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // If no API key is configured, skip auth (open access)
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;

            // Allow static files and the SPA fallback through without auth
            if (!path.StartsWith("/api/", global::System.StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWith("/signalr/", global::System.StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Check header first, then query string
            var providedKey = context.Request.Headers[ApiKeyHeader].ToString();

            if (string.IsNullOrWhiteSpace(providedKey))
            {
                providedKey = context.Request.Query[ApiKeyQuery].ToString();
            }

            if (string.IsNullOrWhiteSpace(providedKey))
            {
                _logger.Warn("Unauthorized API request from {0}: no API key", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"API key required. Set X-Api-Key header or ?apikey= query parameter.\"}");
                return;
            }

            if (!string.Equals(providedKey, _apiKey, global::System.StringComparison.Ordinal))
            {
                _logger.Warn("Unauthorized API request from {0}: invalid API key", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Invalid API key.\"}");
                return;
            }

            await _next(context);
        }
    }
}
