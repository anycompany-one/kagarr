using System.Runtime.InteropServices;
using Kagarr.Common.EnvironmentInfo;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.SystemApi
{
    [V1ApiController]
    public class SystemController : Controller
    {
        [HttpGet("status")]
        public ActionResult<object> GetStatus()
        {
            return new
            {
                AppName = "Kagarr",
                Version = BuildInfo.Version?.ToString() ?? "0.0.0",
                BuildTime = global::System.DateTime.UtcNow,
                IsDebug = BuildInfo.IsDebug,
                IsProduction = RuntimeInfo.IsProduction,
                IsDocker = global::System.IO.File.Exists("/.dockerenv"),
                RuntimeVersion = RuntimeInformation.FrameworkDescription
            };
        }
    }
}
