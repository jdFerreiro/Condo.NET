using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IGlobalFundRepository
{
    Task<GlobalFund?> GetByTypeAsync(GlobalFundType fundType, CancellationToken cancellationToken = default);
    Task UpdateAsync(GlobalFund fund, CancellationToken cancellationToken = default);
}
