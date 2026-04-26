using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class UnitAccountRepository : IUnitAccountRepository
{
    private readonly FinancialDbContext _context;
    public UnitAccountRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<UnitAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.UnitAccounts.FindAsync(new object[] { id }, cancellationToken);

    public async Task<UnitAccount?> GetByExternalUnitIdAsync(string externalUnitId, CancellationToken cancellationToken = default)
        => await _context.UnitAccounts.FirstOrDefaultAsync(u => u.ExternalUnitId == externalUnitId, cancellationToken);

    public async Task<IEnumerable<UnitAccount>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.UnitAccounts.ToListAsync(cancellationToken);

    public async Task AddAsync(UnitAccount unitAccount, CancellationToken cancellationToken = default)
    {
        await _context.UnitAccounts.AddAsync(unitAccount, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UnitAccount unitAccount, CancellationToken cancellationToken = default)
    {
        _context.UnitAccounts.Update(unitAccount);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
