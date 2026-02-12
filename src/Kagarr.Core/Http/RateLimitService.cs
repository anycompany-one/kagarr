using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Kagarr.Core.Http
{
    public interface IRateLimitService
    {
        void WaitAndPulse(string key, TimeSpan interval);
    }

    public class RateLimitService : IRateLimitService
    {
        private readonly ConcurrentDictionary<string, DateTime> _rateLimitStore = new ConcurrentDictionary<string, DateTime>();

        public void WaitAndPulse(string key, TimeSpan interval)
        {
            var now = DateTime.UtcNow;
            var waitUntil = now.Add(interval);

            waitUntil = _rateLimitStore.AddOrUpdate(
                key,
                _ => waitUntil,
                (_, existing) =>
                {
                    var next = existing.Add(interval);
                    return next > waitUntil ? next : waitUntil;
                });

            var delay = waitUntil - DateTime.UtcNow;
            if (delay.TotalMilliseconds > 0)
            {
                Thread.Sleep(delay);
            }
        }
    }
}
