using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Shared.Accounting.DTOs;
using CondoNet.Shared.Accounting.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CondoNet.Accounting.Infrastructure.Consumers
{
    // Implementación homogénea usando el constructor primario de C# 12
    public class BusinessTransactionConsumer(
        IAccountingService accountingService,
        ILogger<BusinessTransactionConsumer> logger) : IConsumer<BusinessTransactionOccurred>
    {
        public async Task Consume(ConsumeContext<BusinessTransactionOccurred> context)
        {
            var message = context.Message;

            // 1. Log Optimizado de Entrada
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Manejando evento contable automático para el Condominio {CondoId}. Tipo Evento: {Type}",
                    message.CondominiumId,
                    message.EventType);
            }

            // Traducir el evento de integración al DTO que espera el autómata
            var automatedRequest = new ProcessAutomatedEntryRequest(
                CondominiumId: message.CondominiumId,
                EventType: message.EventType,
                BaseAmount: message.BaseAmount,
                Description: message.Description,
                DocumentReference: message.DocumentReference
            );

            // Invocar el autómata contable core
            var result = await accountingService.ProcessAutomatedEntryAsync(automatedRequest);

            if (!result.IsSuccess)
            {
                // 2. Log Optimizado de Error
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(
                        "Fallo crítico en el Autómata Contable para la Ref {Ref}: {Error}",
                        message.DocumentReference,
                        result.Error);
                }

                throw new ProcessQueueException($"No se pudo asentar el comprobante automático: {result.Error}");
            }

            // 3. Log Optimizado de Éxito
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Asiento automático generado con éxito. ID Transacción: {TxId}",
                    result.Value);
            }
        }
    }

    // Excepción personalizada para control de reintentos en la cola
    public class ProcessQueueException(string message) : Exception(message);
}
