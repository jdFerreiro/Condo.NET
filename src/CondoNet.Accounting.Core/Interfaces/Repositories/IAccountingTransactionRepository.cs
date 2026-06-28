using CondoNet.Accounting.Core.Entities;

namespace CondoNet.Accounting.Core.Interfaces.Repositories;

public interface IAccountingTransactionRepository
{
    Task<AccountingTransaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<AccountingTransaction>> GetAllAsync(Guid organizationId, Guid condominiumId);
    Task AddAsync(AccountingTransaction transaction);
    Task UpdateAsync(AccountingTransaction transaction);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
