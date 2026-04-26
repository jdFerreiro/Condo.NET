using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IFinancialSubSectionRepository
{
    Task<FinancialSubSection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FinancialSubSection>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(FinancialSubSection section, CancellationToken cancellationToken = default);
    Task UpdateAsync(FinancialSubSection section, CancellationToken cancellationToken = default);
}
