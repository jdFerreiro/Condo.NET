using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Repositories;
using CondoNet.Accounting.Core.Services;

namespace CondoNet.Accounting.Infrastructure.Services;

public class AccountingAutomatonService : IAccountingAutomatonService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IAccountingTransactionRepository _transactionRepository;
    private readonly IAccountingEntryRepository _entryRepository;

    public AccountingAutomatonService(
        IAccountRepository accountRepository,
        IAccountingTransactionRepository transactionRepository,
        IAccountingEntryRepository entryRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _entryRepository = entryRepository;
    }

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
            var account = await _accountRepository.GetByIdAsync(entry.AccountId);
            if (account == null || !account.IsActive)
                throw new InvalidOperationException($"La cuenta {entry.AccountId} no existe o está inactiva.");

            // Actualizar saldo
            account.Balance += entry.Debit - entry.Credit;
            await _accountRepository.UpdateAsync(account);
        }

        // Registrar transacción y asientos
        transaction.CreatedBy = userId;
        transaction.CreatedAt = DateTime.UtcNow;
        await _transactionRepository.AddAsync(transaction);
        foreach (var entry in transaction.Entries)
        {
            entry.TransactionId = transaction.Id;
            await _entryRepository.AddAsync(entry);
        }
    }
}
