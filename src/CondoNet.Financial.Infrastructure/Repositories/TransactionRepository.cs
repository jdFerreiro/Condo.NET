using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class TransactionRepository(FinancialDbContext context) : ITransactionRepository
{
    private readonly FinancialDbContext _context = context;

    public async Task<Transaction> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Transactions.FindAsync([id], cancellationToken);


    public async Task<IEnumerable<Transaction>> GetByUnitAccountIdAsync(Guid unitAccountId, CancellationToken cancellationToken = default)
        => await _context.Transactions.Where(t => t.UnitAccountId == unitAccountId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<Transaction>> GetSplitsByUnitIdAsync(Guid unitAccountId, CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Where(t => t.UnitAccountId == unitAccountId && t.Type == TransactionType.Split)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
