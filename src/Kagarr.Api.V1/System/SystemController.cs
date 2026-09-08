using Kagarr.Common.EnvironmentInfo;
using Kagarr.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kagarr.Api.V1.SystemApi
{
    [V1ApiController]
    public class SystemController : Controller
    {
        // This endpoint is reachable without an API key (Docker HEALTHCHECK),
        // so it must not expose environment details useful for fingerprinting.
        [HttpGet("status")]
        public ActionResult<object> GetStatus()
        {
            return new
            {
                AppName = "Kagarr",
                Version = BuildInfo.Version?.ToString() ?? "0.0.0"
            };
        }
    }
}
