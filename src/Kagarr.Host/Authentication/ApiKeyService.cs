using System;
using Kagarr.Common.Instrumentation;
using NLog;

namespace Kagarr.Host.Authentication
{
    public static class ApiKeyService
    {
        private static readonly Logger Logger = KagarrLogger.GetLogger(typeof(ApiKeyService));

        public static string GetOrCreateApiKey(string dataPath)
        {
            // Check env var first (Docker / manual override)
            var envKey = Environment.GetEnvironmentVariable("KAGARR_API_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
            {
                return envKey;
            }

            // Otherwise, load or generate from file
            var keyPath = global::System.IO.Path.Combine(dataPath, "api_key");

            if (global::System.IO.File.Exists(keyPath))
            {
                var existing = global::System.IO.File.ReadAllText(keyPath).Trim();
                if (!string.IsNullOrWhiteSpace(existing))
                {
                    Environment.SetEnvironmentVariable("KAGARR_API_KEY", existing);
                    return existing;
                }
            }

            // Generate new key
            var newKey = Guid.NewGuid().ToString("N");
            global::System.IO.File.WriteAllText(keyPath, newKey);
            Environment.SetEnvironmentVariable("KAGARR_API_KEY", newKey);

            Logger.Info("Generated new API key. Stored at: {0}", keyPath);

            return newKey;
        }
    }
}
