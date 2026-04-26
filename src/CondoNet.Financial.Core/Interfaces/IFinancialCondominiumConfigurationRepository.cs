using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IFinancialCondominiumConfigurationRepository
{
    Task<FinancialCondominiumConfiguration?> GetByCondominiumIdAsync(Guid condominiumId, CancellationToken cancellationToken = default);
    Task AddAsync(FinancialCondominiumConfiguration config, CancellationToken cancellationToken = default);
    Task UpdateAsync(FinancialCondominiumConfiguration config, CancellationToken cancellationToken = default);
}
