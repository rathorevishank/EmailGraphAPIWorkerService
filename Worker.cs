using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailGraphAPIWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly APIHandler _apiHandler;

        public Worker(ILogger<Worker> logger, APIHandler apiHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiHandler = apiHandler ?? throw new ArgumentNullException(nameof(apiHandler));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                try
                {
                    
                    await _apiHandler.ExecuteLogic(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing API logic.");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
