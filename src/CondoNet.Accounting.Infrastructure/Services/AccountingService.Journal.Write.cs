using CondoNet.Accounting.Core.Entities;
using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;
using CondoNet.Shared.Accounting.Events;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public partial class AccountingService(DbContext context,
		IPublishEndpoint publishEndpoint,
		ITenantService tenantService,
		IHttpContextAccessor httpContextAccessor)

	{
        // 1. REGISTRAR COMPROBANTE DE DIARIO (PARTIDA DOBLE)
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

                    // Afectación del saldo según naturaleza (Deudora o Acreedora)
                    account.CurrentBalance += CalculateBalanceImpact(account.Type, entryDto.Debit, entryDto.Credit);

                    var entry = new AccountingEntry
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organizationId,
                        CondominiumId = condominiumId,
                        AccountingTransactionId = transactionId,
                        AccountId = entryDto.AccountId,
                        Debit = entryDto.Debit,
                        Credit = entryDto.Credit,
                        Reference = entryDto.Reference
                    };

                    transaction.Entries.Add(entry);
                }

                context.Set<AccountingTransaction>().Add(transaction);
                await context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                // Notificación distribuida a RabbitMQ vía MassTransit
                var integrationEvent = new AccountingTransactionPostedEvent(
                    TransactionId: transactionId,
                    CondominiumId: condominiumId,
                    TransactionNumber: transaction.Number,
                    TotalAmount: totalDebit,
                    TransactionDate: transaction.Date,
                    OccurredOn: DateTime.UtcNow
                );

                await publishEndpoint.Publish(integrationEvent, ctx => StampCorrelationId(ctx));

                return Result<Guid>.Success(transactionId);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return Result<Guid>.Failure($"Error crítico al guardar la transacción: {ex.Message}");
            }
        }

        #region Helpers Matemáticos Privados

        private async Task<string> GenerateConsecutiveNumberAsync(Guid condominiumId)
        {
            var prefixDate = DateTime.UtcNow.ToString("yyyyMM");
            var matchPrefix = $"CC-{prefixDate}-";

            var count = await context.Set<AccountingTransaction>()
                .CountAsync(t => t.CondominiumId == condominiumId && t.Number.StartsWith(matchPrefix));
            
            return $"{matchPrefix}{(count + 1).ToString("D4")}";
        }

        #endregion
    }
}
