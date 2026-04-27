using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CondoNET.Financial.Worker
{
    public class MatchingEngineWorker(IMatchingEngine matchingEngine, ILogger<MatchingEngineWorker> logger) : BackgroundService
    {
        private readonly IMatchingEngine _matchingEngine = matchingEngine;
        private readonly ILogger<MatchingEngineWorker> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MatchingEngineWorker iniciado");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _matchingEngine.MatchPaymentsAsync(stoppingToken);
                    _logger.LogInformation("MatchingEngine ejecutado a las {time}", DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en MatchingEngineWorker");
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Ejecuta cada hora
            }
        }
    }
}
