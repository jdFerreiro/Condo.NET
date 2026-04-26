using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IBillingConfigurationRepository
{
    Task<BillingConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BillingConfiguration?> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task AddAsync(BillingConfiguration config, CancellationToken cancellationToken = default);
    Task UpdateAsync(BillingConfiguration config, CancellationToken cancellationToken = default);
}
