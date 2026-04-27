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


                    // Repositorios usados por Matching Engine
                    services.AddScoped<IReportedPaymentRepository, ReportedPaymentRepository>();
                    services.AddScoped<IBankTransactionRepository, BankTransactionRepository>();
                    services.AddScoped<IAuditLogRepository, AuditLogRepository>();

                    services.AddHostedService<DataIntegrityValidationWorker>();

                    // Registrar Matching Engine y su Worker
                    services.AddScoped<IMatchingEngine, MatchingEngineService>();
                    services.AddHostedService<MatchingEngineWorker>();
                })
                .Build();

            host.Run();
        }
    }
}
