using CondoNet.Accounting.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondoNet.Accounting.Core.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<IEnumerable<Account>> GetAllAsync(Guid organizationId, Guid condominiumId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
