using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Shared.Accounting.DTOs;
using CondoNet.Shared.Events.Payments;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CondoNet.Accounting.Infrastructure.Consumers
{
    public class InvoicePaidEventConsumer(
        IAccountingService accountingService,
        DbContext dbContext, // Inyectamos el DbContext de forma homogénea para consultas rápidas
        ILogger<InvoicePaidEventConsumer> logger) : IConsumer<InvoicePaidEvent>
    {
        public async Task Consume(ConsumeContext<InvoicePaidEvent> context)
        {
            var message = context.Message;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Procesando pago automático en contabilidad para factura: {InvoiceId}", message.InvoiceId);
            }

            // 1. RESOLVER EL CONDOMINIUM ID:
            // Buscamos en el Libro Diario un registro previo de emisión (InvoiceRegistered) 
            // que use esta misma factura como referencia, para extraer su CondominiumId de forma segura.
            var previousEntry = await dbContext.Set<AccountingEntry>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Reference != null && e.Reference.Contains(message.InvoiceId.ToString()));

            Guid condominiumId;

            if (previousEntry != null)
            {
                condominiumId = previousEntry.CondominiumId;
            }
            else
            {
                // Si la factura no tiene un asiento previo, registramos el error y abortamos
                // para que MassTransit envíe el mensaje a la cola de error (error queue) para auditoría
                logger.LogError("No se pudo procesar el pago. No existe un registro contable previo para la factura {InvoiceId}", message.InvoiceId);
                throw new ProcessQueueException($"Condominio no encontrado para la factura {message.InvoiceId}");
            }

            // 2. MAPEAR AL DTO UNIFICADO UTILIZANDO LAS PROPIEDADES DE TU MENSAJE REAL
            var automatedRequest = new ProcessAutomatedEntryRequest(
                CondominiumId: condominiumId, // Llave foránea resuelta en background
                EventType: 2, // 2 = InvoicePaymentReceived
                BaseAmount: message.Amount, // <-- Reemplaza por tu propiedad real (ej: message.Amount o message.Total)
                Description: $"Recaudación de Pago - Factura Ref: {message.InvoiceId}",
                DocumentReference: message.PaymentReference // <-- Reemplaza por tu propiedad real de referencia de pago
            );

            // 3. ENVIAR AL AUTÓMATA CONTABLE CORE
            var result = await accountingService.ProcessAutomatedEntryAsync(automatedRequest);

            if (!result.IsSuccess)
            {
                throw new ProcessQueueException($"Fallo en el autómata al registrar pago: {result.Error}");
            }
        }
    }
}
