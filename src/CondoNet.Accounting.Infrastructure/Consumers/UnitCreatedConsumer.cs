using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Shared.Accounting.DTOs;
using CondoNet.Shared.Asset.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CondoNet.Accounting.Infrastructure.Consumers
{
    // Clase unificada que escucha tanto cargas masivas como registros individuales
    public class UnitCreatedConsumer(
        IAccountingService accountingService,
        ILogger<UnitCreatedConsumer> logger)
        : IConsumer<UnitsImported>, IConsumer<UnitCreated>
    {
        // ESCENARIO A: PROCESAMIENTO MASIVO (BULK IMPORT)
        public async Task Consume(ConsumeContext<UnitsImported> context)
        {
            var message = context.Message;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Background Process: Detectada importación masiva de {Count} unidades para el Condominio {CondoId}.",
                    message.Units.Count,
                    message.CondominiumId);
            }

            // Aquí procesas la lógica financiera masiva si vienen con deudas iniciales
            await ProcessUnitFinancialOnboardingAsync(message.CondominiumId, 0, $"Carga Masiva Unidades");
        }

        // ESCENARIO B: PROCESAMIENTO INDIVIDUAL (FORMULARIO WEB)
        public async Task Consume(ConsumeContext<UnitCreated> context)
        {
            var message = context.Message;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Background Process: Detectada creación de Unidad Individual '{Identifier}' para el Condominio {CondoId}.",
                    message.Identifier,
                    message.CondominiumId);
            }

            // Procesas la inicialización financiera para este departamento específico
            await ProcessUnitFinancialOnboardingAsync(message.CondominiumId, 0, $"Alta Individual: {message.Identifier}");
        }

        #region Lógica Unificada de Onboarding Financiero

        private async Task ProcessUnitFinancialOnboardingAsync(Guid condominiumId, decimal initialBalance, string reference)
        {
            if (initialBalance > 0)
            {
                var automatedRequest = new ProcessAutomatedEntryRequest(
                    CondominiumId: condominiumId,
                    EventType: 5, // Tipo de evento preconfigurado para "Saldos Iniciales"
                    BaseAmount: initialBalance,
                    Description: $"Inicialización de Cuenta Corriente - {reference}",
                    DocumentReference: $"ONB-{DateTime.UtcNow:yyyyMMdd}"
                );

                var result = await accountingService.ProcessAutomatedEntryAsync(automatedRequest);

                if (!result.IsSuccess)
                {
                    throw new ProcessQueueException($"Error al asentar finanzas de unidad en background: {result.Error}");
                }
            }
        }

        #endregion
    }
}
