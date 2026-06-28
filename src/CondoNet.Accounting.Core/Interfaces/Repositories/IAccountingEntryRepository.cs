using CondoNet.Accounting.Core.Entities;

namespace CondoNet.Accounting.Core.Interfaces.Repositories;

public interface IAccountingEntryRepository
{
    Task<AccountingEntry?> GetByIdAsync(Guid id);
    Task<IEnumerable<AccountingEntry>> GetAllByTransactionAsync(Guid transactionId);
    Task AddAsync(AccountingEntry entry);
    Task UpdateAsync(AccountingEntry entry);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
