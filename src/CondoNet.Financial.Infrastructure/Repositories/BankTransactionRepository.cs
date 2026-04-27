using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class BankTransactionRepository : IBankTransactionRepository
{
    private readonly FinancialDbContext _context;
    public BankTransactionRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BankTransaction>> GetUnmatchedAsync(CancellationToken cancellationToken = default)
        => await _context.Set<BankTransaction>()
            .Where(t => !t.IsMatched)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(BankTransaction transaction, CancellationToken cancellationToken = default)
    {
        _context.BankTransactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
