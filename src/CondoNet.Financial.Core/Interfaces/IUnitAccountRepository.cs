using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IUnitAccountRepository
{
    Task<UnitAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UnitAccount?> GetByExternalUnitIdAsync(string externalUnitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UnitAccount>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(UnitAccount unitAccount, CancellationToken cancellationToken = default);
    Task UpdateAsync(UnitAccount unitAccount, CancellationToken cancellationToken = default);
}
