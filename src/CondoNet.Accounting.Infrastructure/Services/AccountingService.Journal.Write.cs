using CondoNet.Accounting.Core.Entities;
using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;
using CondoNet.Shared.Accounting.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public partial class AccountingService
    {
        public async Task<Result<Guid>> ProcessAutomatedEntryAsync(ProcessAutomatedEntryRequest request)
        {
            var businessEvent = (BusinessEventType)request.EventType;

            // Buscar la plantilla configurada para este evento de negocio en el condominio
            var template = await context.Set<AccountingTemplate>()
                .Include(t => t.Rules)
                .FirstOrDefaultAsync(t => t.CondominiumId == request.CondominiumId && t.EventType == businessEvent);

            if (template == null)
                return Result<Guid>.Failure($"No existe una plantilla contable configurada para el evento: {businessEvent}.");

            if (template.Rules == null || template.Rules.Count == 0)
                return Result<Guid>.Failure("La plantilla contable no contiene reglas de distribución.");

            // Validar consistencia matemática de la plantilla (Debe == Haber en factores)
            decimal totalDebitFactor = template.Rules.Where(r => r.Movement == RuleMovementType.Debit).Sum(r => r.PercentageFactor);
            decimal totalCreditFactor = template.Rules.Where(r => r.Movement == RuleMovementType.Credit).Sum(r => r.PercentageFactor);

            if (totalDebitFactor != totalCreditFactor)
                return Result<Guid>.Failure($"La plantilla '{template.Name}' está mal configurada. Los factores del Debe ({totalDebitFactor}) y Haber ({totalCreditFactor}) no coinciden.");

            // Generar dinámicamente los renglones (Entries) aplicando los factores sobre el Monto Base
            var calculatedEntries = new List<CreateEntryRequest>();

            foreach (var rule in template.Rules)
            {
                // Aplicar factor: Ej. $1000 base * 0.16 factor = $160 de Impuesto
                decimal calculatedAmount = Math.Round(request.BaseAmount * rule.PercentageFactor, 4);

                decimal debit = rule.Movement == RuleMovementType.Debit ? calculatedAmount : 0;
                decimal credit = rule.Movement == RuleMovementType.Credit ? calculatedAmount : 0;

                var entryRequest = new CreateEntryRequest(
                    AccountId: rule.AccountId,
                    Debit: debit,
                    Credit: credit,
                    Reference: $"[Autómata - {template.Name}] Ref: {request.DocumentReference}"
                );

                calculatedEntries.Add(entryRequest);
            }

            // Construir la solicitud de transacción unificada
            var transactionRequest = new CreateTransactionRequest(
                Description: $"{request.Description} (Ref: {request.DocumentReference})",
                Date: DateTime.UtcNow,
                Entries: calculatedEntries
            );

            // Invocación local directa garantizada al estar en el mismo archivo físico
            return await CreateTransactionAsync(transactionRequest);
        }

        // 2. REGISTRAR COMPROBANTE DE DIARIO (PARTIDA DOBLE)
        public async Task<Result<Guid>> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var organizationId = tenantService.GetOrganizationId();
            var condominiumId = tenantService.GetCondominiumId();
            var username = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

            if (request.Entries == null || request.Entries.Count < 2)
                return Result<Guid>.Failure("Una transacción debe tener al menos dos renglones (Partida Doble).");

            // Validar balance de centavos: Débito == Crédito
            decimal totalDebit = request.Entries.Sum(e => e.Debit);
            decimal totalCredit = request.Entries.Sum(e => e.Credit);

            if (totalDebit != totalCredit)
                return Result<Guid>.Failure($"Asiento descuadrado. Débito: {totalDebit} | Crédito: {totalCredit}");

            using var dbTransaction = await context.Database.BeginTransactionAsync();
            try
            {
                var transactionId = Guid.NewGuid();
                var nextNumber = await GenerateConsecutiveNumberAsync(condominiumId);

                var transaction = new AccountingTransaction
                {
                    Id = transactionId,
                    OrganizationId = organizationId,
                    CondominiumId = condominiumId,
                    Number = nextNumber,
                    Description = request.Description,
                    Date = request.Date,
                    Status = TransactionStatus.Posted,
                    CreatedBy = username
                };

                foreach (var entryDto in request.Entries)
                {
                    var account = await context.Set<Account>()
                        .FirstOrDefaultAsync(a => a.Id == entryDto.AccountId && a.CondominiumId == condominiumId);

                    if (account == null)
                        return Result<Guid>.Failure($"La cuenta con ID {entryDto.AccountId} no existe.");
                    if (!account.IsActive)
                        return Result<Guid>.Failure($"La cuenta '{account.Name}' está inactiva.");
                    if (!account.IsTransactional)
                        return Result<Guid>.Failure($"La cuenta '{account.Name}' es de agrupación y no acepta movimientos.");

                    // Afectación del saldo según naturaleza
                    account.CurrentBalance += CalculateBalanceImpactLocal(account.Type, entryDto.Debit, entryDto.Credit);

                    var entry = new AccountingEntry
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organizationId,
                        CondominiumId = condominiumId,
                        AccountingTransactionId = transactionId,
                        AccountId = entryDto.AccountId,
                        Debit = entryDto.Debit,
                        Credit = entryDto.Credit,
                        Reference = entryDto.Reference ?? "N/A"
                    };

                    transaction.Entries.Add(entry);
                }

                context.Set<AccountingTransaction>().Add(transaction);
                await context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                // Notificación distribuida a RabbitMQ vía MassTransit especificando el tipo genérico
                var integrationEvent = new AccountingTransactionPostedEvent(
                    TransactionId: transactionId,
                    CondominiumId: condominiumId,
                    TransactionNumber: transaction.Number,
                    TotalAmount: totalDebit,
                    TransactionDate: transaction.Date,
                    OccurredOn: DateTime.UtcNow
                );

                // Agregamos el tipo genérico <AccountingTransactionPostedEvent> antes del paréntesis
                await publishEndpoint.Publish<AccountingTransactionPostedEvent>(integrationEvent, ctx => StampCorrelationId(ctx));

                return Result<Guid>.Success(transactionId);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return Result<Guid>.Failure($"Error crítico al guardar la transacción: {ex.Message}");
            }
        }

        #region Helpers Privados de Numeración Correlativa

        private async Task<string> GenerateConsecutiveNumberAsync(Guid condominiumId)
        {
            var prefixDate = DateTime.UtcNow.ToString("yyyyMM");
            var matchPrefix = $"CC-{prefixDate}-";

            var count = await context.Set<AccountingTransaction>()
                .CountAsync(t => t.CondominiumId == condominiumId && t.Number.StartsWith(matchPrefix));

            return $"{matchPrefix}{count + 1:D4}";
        }

        #endregion
    }
}
