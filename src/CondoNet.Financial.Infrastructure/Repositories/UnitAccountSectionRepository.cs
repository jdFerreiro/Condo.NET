using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class UnitAccountSectionRepository : IUnitAccountSectionRepository
{
    private readonly FinancialDbContext _context;
    public UnitAccountSectionRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<UnitAccountSection?> GetByIdsAsync(Guid unitAccountId, Guid financialSubSectionId, CancellationToken cancellationToken = default)
        => await _context.UnitAccountSections.FirstOrDefaultAsync(u => u.UnitAccountId == unitAccountId && u.FinancialSubSectionId == financialSubSectionId, cancellationToken);

    public async Task<IEnumerable<UnitAccountSection>> GetByUnitAccountIdAsync(Guid unitAccountId, CancellationToken cancellationToken = default)
        => await _context.UnitAccountSections.Where(u => u.UnitAccountId == unitAccountId).ToListAsync(cancellationToken);

    public async Task<IEnumerable<UnitAccountSection>> GetByFinancialSubSectionIdAsync(Guid financialSubSectionId, CancellationToken cancellationToken = default)
        => await _context.UnitAccountSections.Where(u => u.FinancialSubSectionId == financialSubSectionId).ToListAsync(cancellationToken);

    public async Task AddAsync(UnitAccountSection assignment, CancellationToken cancellationToken = default)
    {
        await _context.UnitAccountSections.AddAsync(assignment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UnitAccountSection assignment, CancellationToken cancellationToken = default)
    {
        _context.UnitAccountSections.Update(assignment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
