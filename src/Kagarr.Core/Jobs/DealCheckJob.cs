using System;
using System.Threading;
using System.Threading.Tasks;
using Kagarr.Common.Instrumentation;
using Kagarr.Core.Deals;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Kagarr.Core.Jobs
{
    public class DealCheckJob : BackgroundService
    {
        private readonly IDealService _dealService;
        private readonly Logger _logger;
        private readonly TimeSpan _interval;

        public DealCheckJob(IDealService dealService)
        {
            _dealService = dealService;
            _logger = KagarrLogger.GetLogger(this);

            var hoursEnv = global::System.Environment.GetEnvironmentVariable("KAGARR_DEAL_CHECK_HOURS");
            var hours = 6;
            if (!string.IsNullOrWhiteSpace(hoursEnv) && int.TryParse(hoursEnv, out var parsed) && parsed > 0)
            {
                hours = parsed;
            }

            _interval = TimeSpan.FromHours(hours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info("Deal check scheduler started. Interval: {0} hours", _interval.TotalHours);

            // Wait 2 minutes after startup before the first check
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.Info("Running scheduled deal check");
                    _dealService.CheckAllDeals();
                    _logger.Info("Scheduled deal check complete");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Scheduled deal check failed");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
