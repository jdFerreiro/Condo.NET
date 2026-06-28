using CondoNet.Accounting.Core.Entities;

namespace CondoNet.Accounting.Core.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<IEnumerable<Account>> GetAllAsync(Guid organizationId, Guid condominiumId);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
