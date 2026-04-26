using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IUnitAccountSectionRepository
{
    Task<UnitAccountSection?> GetByIdsAsync(Guid unitAccountId, Guid financialSubSectionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitAccountSection>> GetByUnitAccountIdAsync(Guid unitAccountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitAccountSection>> GetByFinancialSubSectionIdAsync(Guid financialSubSectionId, CancellationToken cancellationToken = default);
    Task AddAsync(UnitAccountSection assignment, CancellationToken cancellationToken = default);
    Task UpdateAsync(UnitAccountSection assignment, CancellationToken cancellationToken = default);
}
