using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Interfaces.Repositories;
using CondoNet.Accounting.Core.Interfaces.Services;

namespace CondoNet.Accounting.Infrastructure.Services;

public class AccountingAutomatonService(
        IAccountRepository accountRepository,
        IAccountingService accountingService,
        IAccountingTransactionRepository transactionRepository,
        IAccountingEntryRepository entryRepository) : IAccountingAutomatonService
{
    public async Task ProcessTransactionAsync(AccountingTransaction transaction, Guid userId)
    {
        // Validar que la transacción esté balanceada
        var totalDebit = transaction.Entries.Sum(e => e.Debit);
        var totalCredit = transaction.Entries.Sum(e => e.Credit);
        if (totalDebit != totalCredit)
            throw new InvalidOperationException("La transacción no está balanceada (debe ≠ haber).");

        // Validar cuentas y actualizar saldos
        foreach (var entry in transaction.Entries)
        {
            var account = await accountRepository.GetByIdAsync(entry.AccountId);
            if (account == null || !account.IsActive)
                throw new InvalidOperationException($"La cuenta {entry.AccountId} no existe o está inactiva.");

            // Actualizar saldo
            account.CurrentBalance += accountingService.CalculateBalanceImpactLocal(account.Type, entry.Debit, entry.Credit);
            await accountRepository.UpdateAsync(account);
        }

        // Registrar transacción y asientos
        transaction.CreatedBy = userId.ToString();
        transaction.CreatedAt = DateTime.UtcNow;
        await transactionRepository.AddAsync(transaction);
        foreach (var entry in transaction.Entries)
        {
            entry.AccountingTransactionId = transaction.Id;
            await entryRepository.AddAsync(entry);
        }
    }
}
