using CondoNet.Financial.Core.Services;
using CondoNet.Financial.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CondoNET.Financial.Worker
{
    public class DataIntegrityValidationWorker(DataIntegrityValidatorService validator, ILogger<DataIntegrityValidationWorker> logger) : BackgroundService
    {
        private readonly DataIntegrityValidatorService _validator = validator;
        private readonly ILogger<DataIntegrityValidationWorker> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DataIntegrityValidationWorker iniciado");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _validator.ValidateMerkleRootsAsync(stoppingToken);
                    await _validator.ValidateSplitsAsync(stoppingToken);
                    // Puedes agregar más validaciones aquí (pagos, fondos, etc.)
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Validación de integridad completada a las {time}", DateTime.UtcNow);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la validación de integridad de datos");
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Ejecuta cada 24h
            }
        }
    }
}
