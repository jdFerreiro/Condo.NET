using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class CondoExpenseRepository : ICondoExpenseRepository
{
    private readonly FinancialDbContext _context;
    public CondoExpenseRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<CondoExpense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.CondoExpenses.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IEnumerable<CondoExpense>> GetByPeriodAsync(int month, int year, CancellationToken cancellationToken = default)
        => await _context.CondoExpenses.Where(e => e.Month == month && e.Year == year).ToListAsync(cancellationToken);

    public async Task AddAsync(CondoExpense expense, CancellationToken cancellationToken = default)
    {
        await _context.CondoExpenses.AddAsync(expense, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CondoExpense expense, CancellationToken cancellationToken = default)
    {
        _context.CondoExpenses.Update(expense);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
