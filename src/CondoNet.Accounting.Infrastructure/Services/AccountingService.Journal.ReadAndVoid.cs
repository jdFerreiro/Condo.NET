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
        // 1. OBTENER DETALLE PROFUNDO DE UN COMPROBANTE
        public async Task<Result<TransactionDetailResponse>> GetTransactionByIdAsync(Guid transactionId)
        {
            var condominiumId = tenantService.GetCondominiumId();

            var tx = await context.Set<AccountingTransaction>()
                .Include(t => t.Entries).ThenInclude(e => e.Account)
                .FirstOrDefaultAsync(t => t.Id == transactionId && t.CondominiumId == condominiumId);

            if (tx == null)
                return Result<TransactionDetailResponse>.Failure("Comprobante de diario no encontrado.");

            var details = new TransactionDetailResponse(
                tx.Id, tx.Number, tx.Description, tx.Date, tx.Status.ToString(), tx.CreatedBy,
                [.. tx.Entries.Select(e => new EntryDetailResponse(
                    e.Id, e.AccountId, e.Account.Code, e.Account.Name, e.Debit, e.Credit, e.Reference))]
            );

            return Result<TransactionDetailResponse>.Success(details);
        }

        // 2. CONSULTAR LIBRO DIARIO POR RANGO DE FECHAS
        public async Task<Result<List<TransactionSummaryResponse>>> GetJournalEntriesAsync(DateTime from, DateTime to)
        {
            var condominiumId = tenantService.GetCondominiumId();

            var list = await context.Set<AccountingTransaction>()
                .Include(t => t.Entries)
                .Where(t => t.CondominiumId == condominiumId && t.Date >= from && t.Date <= to)
                .OrderByDescending(t => t.Date).ThenByDescending(t => t.Number)
                .Select(t => new TransactionSummaryResponse(
                    t.Id, t.Number, t.Description, t.Date, t.Status.ToString(), t.Entries.Sum(e => e.Debit), t.CreatedBy))
                .ToListAsync();

            return Result<List<TransactionSummaryResponse>>.Success(list);
        }

        // 3. ANULACIÓN Y REVERSIÓN DE IMPACTO CONTABLE
        public async Task<Result<bool>> VoidTransactionAsync(Guid transactionId)
        {
            var condominiumId = tenantService.GetCondominiumId();

            using var dbTransaction = await context.Database.BeginTransactionAsync();
            try
            {
                var tx = await context.Set<AccountingTransaction>()
                    .Include(t => t.Entries).ThenInclude(e => e.Account)
                    .FirstOrDefaultAsync(t => t.Id == transactionId && t.CondominiumId == condominiumId);

                if (tx == null)
                    return Result<bool>.Failure("Comprobante no encontrado.");
                if (tx.Status == TransactionStatus.Voided)
                    return Result<bool>.Failure("El comprobante ya se encuentra anulado.");

                // Reversión: Restamos el impacto original aplicando los montos de forma invertida
                foreach (var entry in tx.Entries)
                {
                    entry.Account.CurrentBalance -= CalculateBalanceImpactLocal(entry.Account.Type, entry.Debit, entry.Credit);
                }

                tx.Status = TransactionStatus.Voided;
                await context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                // Publicar evento en RabbitMQ para notificar a los módulos dependientes (vía MassTransit)
                var integrationEvent = new AccountingTransactionVoidedEvent(
                    TransactionId: tx.Id,
                    CondominiumId: condominiumId,
                    TransactionNumber: tx.Number,
                    OccurredOn: DateTime.UtcNow
                );

                await publishEndpoint.Publish(integrationEvent, ctx => StampCorrelationId(ctx));

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return Result<bool>.Failure($"Error al procesar la anulación: {ex.Message}");
            }
        }
    }
}


