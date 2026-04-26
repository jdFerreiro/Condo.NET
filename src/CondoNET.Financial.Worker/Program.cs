using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CondoNet.Financial.Core.Services;

namespace CondoNET.Financial.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Registrar dependencias del Core y de infraestructura
                    services.AddScoped<DataIntegrityValidatorService>();

                    // Aquí debes registrar los repositorios e infraestructura necesarios
                    // Ejemplo:
                    // services.AddScoped<IInvoiceRepository, InvoiceRepository>();
                    // services.AddScoped<IBlockchainIntegrationService, BlockchainIntegrationService>();
                    // services.AddScoped<IAuditLogRepository, AuditLogRepository>();
                    // services.AddScoped<IUnitAccountRepository, UnitAccountRepository>();
                    // services.AddScoped<ITransactionRepository, TransactionRepository>();

                    services.AddHostedService<DataIntegrityValidationWorker>();
                })
                .Build();

            host.Run();
        }
    }
}
