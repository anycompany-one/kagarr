using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Microsoft.AspNetCore.Http;
using NLog;

namespace Kagarr.Host.Authentication
{
    public class ApiKeyMiddleware
    {
        private const string ApiKeyHeader = "X-Api-Key";

        private readonly RequestDelegate _next;
        private readonly Logger _logger;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
            _logger = KagarrLogger.GetLogger(typeof(ApiKeyMiddleware));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Read key lazily on each request to avoid race condition with ApiKeyService
            var apiKey = global::System.Environment.GetEnvironmentVariable("KAGARR_API_KEY");

            // If no API key is configured, skip auth (open access)
            if (string.IsNullOrWhiteSpace(apiKey))
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

            // Allow system status endpoint without auth (used by Docker HEALTHCHECK).
            // The endpoint only returns the app name and version.
            if (path.Equals("/api/v1/system/status", global::System.StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Allow media cover images without auth. The frontend loads them via plain
            // <img src> tags, which cannot send the X-Api-Key header. Covers are
            // non-sensitive (public IGDB artwork) and the controller validates the
            // file name against path traversal.
            if (HttpMethods.IsGet(context.Request.Method) &&
                path.StartsWith("/api/v1/mediacover/", global::System.StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Only the X-Api-Key header is accepted; query-string keys leak into
            // logs, proxies and browser history.
            var providedKey = context.Request.Headers[ApiKeyHeader].ToString();

            if (string.IsNullOrWhiteSpace(providedKey))
            {
                _logger.Warn("Unauthorized API request from {0}: no API key", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"API key required. Set the X-Api-Key header.\"}");
                return;
            }

            if (!FixedTimeEquals(providedKey, apiKey))
            {
                _logger.Warn("Unauthorized API request from {0}: invalid API key", context.Connection.RemoteIpAddress);
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\":\"Invalid API key.\"}");
                return;
            }

            await _next(context);
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            var aBytes = Encoding.UTF8.GetBytes(a);
            var bBytes = Encoding.UTF8.GetBytes(b);
            return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
        }
    }
}
