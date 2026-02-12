using System;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Datastore;
using Microsoft.AspNetCore.Http;
using NLog;

namespace Kagarr.Host.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Logger _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
            _logger = KagarrLogger.GetLogger(typeof(ExceptionHandlingMiddleware));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ModelNotFoundException ex)
            {
                _logger.Warn(ex, "Resource not found: {0} {1}", context.Request.Method, context.Request.Path);
                context.Response.StatusCode = 404;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    $"{{\"error\":\"{EscapeJson(ex.Message)}\",\"requestId\":\"{context.TraceIdentifier}\"}}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled exception on {0} {1}", context.Request.Method, context.Request.Path);
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    $"{{\"error\":\"An internal error occurred\",\"requestId\":\"{context.TraceIdentifier}\"}}");
            }
        }

        private static string EscapeJson(string value)
        {
            return value?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? string.Empty;
        }
    }
}
