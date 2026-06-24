using CondoNet.Accounting.Core.Interfaces;
using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Accounting.Infrastructure.Consumers;
using CondoNet.Accounting.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CondoNet.Accounting.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. REGISTRO DEL CORE CONTABLE (Saldos, Libro Diario, Autómata y Reportes)
            // Vincula la interfaz con la implementación unificada por tus archivos parciales
            services.AddScoped<IAccountingService, AccountingService>();

            // 2. REGISTRO DEL MOTOR DE SIEMBRA CONTABLE AUTOMÁTICA
            services.AddScoped<IAccountingSeeder, AccountingSeeder>();

            // Registro del servicio de mantenimiento de plantillas del autómata
            services.AddScoped<IAccountingTemplateService, AccountingTemplateService>();

            // 3. Configuración homogénea de MassTransit para CondoNet
            services.AddMassTransit(x =>
            {
                x.AddConsumer<BusinessTransactionConsumer>();
                x.AddConsumer<CondominiumCreatedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitUri = configuration.GetValue<string>("RabbitMQ:Uri") ?? "rabbitmq://localhost";
                    cfg.Host(new Uri(rabbitUri), h =>
                    {
                        h.Username(configuration.GetValue<string>("RabbitMQ:Username") ?? "guest");
                        h.Password(configuration.GetValue<string>("RabbitMQ:Password") ?? "guest");
                    });

                    cfg.ReceiveEndpoint("accounting-business-transactions", e =>
                    {
                        e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)));
                        e.ConcurrentMessageLimit = 4;
                        e.ConfigureConsumer<BusinessTransactionConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("accounting-condominium-onboarding", e =>
                    {
                        e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10)));
                        e.ConcurrentMessageLimit = 2;
                        e.ConfigureConsumer<CondominiumCreatedConsumer>(context);
                    });

                    // Cola 3: Endpoint dedicado al procesamiento y auditoría de unidades importadas/creadas
                    cfg.ReceiveEndpoint("accounting-unit-initialization", e =>
                    {
                        e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)));
                        e.ConcurrentMessageLimit = 2;

                        // Enlazar el consumidor
                        e.ConfigureConsumer<UnitCreatedConsumer>(context);
                    });
                });
            });

            return services;
        }
    }
}
