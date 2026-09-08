using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Kagarr.Host.Authentication;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace Kagarr.Host.Test.Authentication
{
    [TestFixture]
    public class ApiKeyMiddlewareTests
    {
        private const string ApiKey = "test-api-key";

        private string _originalApiKey;

        [SetUp]
        public void Setup()
        {
            _originalApiKey = global::System.Environment.GetEnvironmentVariable("KAGARR_API_KEY");
            global::System.Environment.SetEnvironmentVariable("KAGARR_API_KEY", ApiKey);
        }

        [TearDown]
        public void TearDown()
        {
            global::System.Environment.SetEnvironmentVariable("KAGARR_API_KEY", _originalApiKey);
        }

        [Test]
        public async Task should_reject_api_docs_without_api_key()
        {
            var context = CreateContext("/api/docs/v1.json");

            var nextCalled = await Invoke(context);

            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(401);
        }

        [Test]
        public async Task should_allow_api_docs_with_valid_api_key()
        {
            var context = CreateContext("/api/docs/v1.json");
            context.Request.Headers["X-Api-Key"] = ApiKey;

            var nextCalled = await Invoke(context);

            nextCalled.Should().BeTrue();
        }

        [Test]
        public async Task should_allow_system_status_without_api_key()
        {
            var context = CreateContext("/api/v1/system/status");

            var nextCalled = await Invoke(context);

            nextCalled.Should().BeTrue();
        }

        [Test]
        public async Task should_allow_api_request_with_valid_header()
        {
            var context = CreateContext("/api/v1/game");
            context.Request.Headers["X-Api-Key"] = ApiKey;

            var nextCalled = await Invoke(context);

            nextCalled.Should().BeTrue();
        }

        [Test]
        public async Task should_not_accept_api_key_from_query_string()
        {
            var context = CreateContext("/api/v1/game");
            context.Request.QueryString = new QueryString("?apikey=" + ApiKey);

            var nextCalled = await Invoke(context);

            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(401);
        }

        [Test]
        public async Task should_reject_invalid_api_key()
        {
            var context = CreateContext("/api/v1/game");
            context.Request.Headers["X-Api-Key"] = "wrong-key";

            var nextCalled = await Invoke(context);

            nextCalled.Should().BeFalse();
            context.Response.StatusCode.Should().Be(401);
        }

        [Test]
        public async Task should_allow_static_files_without_api_key()
        {
            var context = CreateContext("/index.html");

            var nextCalled = await Invoke(context);

            nextCalled.Should().BeTrue();
        }

        [Test]
        public async Task should_allow_all_requests_when_no_api_key_is_configured()
        {
            global::System.Environment.SetEnvironmentVariable("KAGARR_API_KEY", null);

            var context = CreateContext("/api/v1/game");

            var nextCalled = await Invoke(context);

            nextCalled.Should().BeTrue();
        }

        private static DefaultHttpContext CreateContext(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Response.Body = new MemoryStream();
            return context;
        }

        private static async Task<bool> Invoke(DefaultHttpContext context)
        {
            var nextCalled = false;
            var middleware = new ApiKeyMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            await middleware.InvokeAsync(context);

            return nextCalled;
        }
    }
}
