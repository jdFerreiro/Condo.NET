using CondoNet.Accounting.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondoNet.Accounting.Core.Repositories;

public interface IAccountingTransactionRepository
{
    Task<AccountingTransaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<AccountingTransaction>> GetAllAsync(Guid organizationId, Guid condominiumId);
    Task AddAsync(AccountingTransaction transaction);
    Task UpdateAsync(AccountingTransaction transaction);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
