using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface ICurrencyAdjustmentLogRepository
{
    Task AddAsync(CurrencyAdjustmentLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<CurrencyAdjustmentLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
}
