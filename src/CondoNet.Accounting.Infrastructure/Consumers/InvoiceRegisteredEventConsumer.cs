using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Shared.Accounting.DTOs;
using CondoNet.Shared.Events.Payments;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CondoNet.Accounting.Infrastructure.Consumers
{
    public class InvoiceRegisteredEventConsumer(
        IAccountingService accountingService,
        DbContext dbContext, // Inyectamos el DbContext de forma homogénea para resolver el condominio
        ILogger<InvoiceRegisteredEventConsumer> logger) : IConsumer<InvoiceRegisteredEvent>
    {
        public async Task Consume(ConsumeContext<InvoiceRegisteredEvent> context)
        {
            var message = context.Message;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Procesando emisión automática de cuenta por cobrar para factura: {InvoiceId}", message.InvoiceId);
            }

            // 1. RESOLVER EL CONDOMINIUM ID DESDE EL MAESTRO DE CUENTAS DEL INQUILINO:
            // Buscamos una cuenta contable activa de agrupación o transaccional de cobranza 
            // que sirva como ancla para recuperar de forma segura el CondominiumId del inquilino actual.
            var accountAnchor = await dbContext.Set<Account>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Code.StartsWith("1.1.02") && a.IsActive); // Código base de Cuentas por Cobrar

            Guid condominiumId;

            if (accountAnchor != null)
            {
                condominiumId = accountAnchor.CondominiumId;
            }
            else
            {
                // Si el catálogo está vacío o no se ha inicializado el condominio, abortamos con reintento
                logger.LogError("Fallo en Onboarding: No se encontró un Condominio inicializado con plan de cuentas para procesar la Factura {InvoiceId}", message.InvoiceId);
                throw new ProcessQueueException($"No se pudo resolver el Condominio para la factura {message.InvoiceId}. Verifique el sembrado.");
            }

            // 2. MAPEAR AL DTO UNIFICADO AJUSTANDO A TUS PROPIEDADES REALES
            var automatedRequest = new ProcessAutomatedEntryRequest(
                CondominiumId: condominiumId, // ID del condominio resuelto dinámicamente
                EventType: 1, // 1 = MonthlyBillingGenerated
                BaseAmount: message.Amount, // <-- Reemplaza por tu propiedad real (ej: message.Amount o message.Total)
                Description: $"Emisión Mensual de Gastos de Condominio",
                DocumentReference: message.InvoiceId.ToString() // <-- Reemplaza por tu propiedad real de número o ID
            );

            // 3. ENVIAR AL AUTÓMATA CONTABLE CORE
            var result = await accountingService.ProcessAutomatedEntryAsync(automatedRequest);

            if (!result.IsSuccess)
            {
                throw new ProcessQueueException($"Fallo en el autómata al registrar emisión: {result.Error}");
            }
        }
    }
}
