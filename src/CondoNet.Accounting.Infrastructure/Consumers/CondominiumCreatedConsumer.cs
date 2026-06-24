using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Shared.Asset.Events; // Namespace de tus eventos de Asset
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CondoNet.Accounting.Infrastructure.Consumers
{
    // Implementación homogénea usando el constructor primario de C# 12
    public class CondominiumCreatedConsumer(
        IAccountingSeeder accountingSeeder,
        ILogger<CondominiumCreatedConsumer> logger) : IConsumer<CondominiumCreatedEvent>
    {
        public async Task Consume(ConsumeContext<CondominiumCreatedEvent> context)
        {
            var message = context.Message;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Background Process: Detectado nuevo condominio desde el Bus. Inicializando catálogo base. CondoId: {CondoId}",
                    message.CondominiumId);
            }

            // Invocar de manera segura al sembrador contable con los identificadores del evento
            var result = await accountingSeeder.SeedBaseCatalogAsync(
                tenantCondoId: message.CondominiumId,
                tenantOrgId: message.OrganizationId
            );

            if (!result.IsSuccess)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(
                        "Fallo en proceso diferido de siembra para Condominio {CondoId}: {Error}",
                        message.CondominiumId,
                        result.Error);
                }

                // Excepción explícita para activar las políticas de reintento de RabbitMQ
                throw new ProcessQueueException($"No se pudo inicializar el árbol contable básico: {result.Error}");
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "¡Proceso completado con éxito! Estructura contable base creada para Condominio {CondoId}",
                    message.CondominiumId);
            }
        }
    }
}
