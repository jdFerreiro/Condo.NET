using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface ICondoExpenseRepository
{
    Task<CondoExpense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CondoExpense>> GetByPeriodAsync(int month, int year, CancellationToken cancellationToken = default);
    Task AddAsync(CondoExpense expense, CancellationToken cancellationToken = default);
    Task UpdateAsync(CondoExpense expense, CancellationToken cancellationToken = default);
}
