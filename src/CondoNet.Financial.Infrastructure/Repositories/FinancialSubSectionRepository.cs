using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class FinancialSubSectionRepository(FinancialDbContext context) : IFinancialSubSectionRepository
{
    private readonly FinancialDbContext _context = context;

    public async Task<FinancialSubSection> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.FinancialSubSections.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<FinancialSubSection>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.FinancialSubSections.ToListAsync(cancellationToken);

    public async Task AddAsync(FinancialSubSection section, CancellationToken cancellationToken = default)
    {
        await _context.FinancialSubSections.AddAsync(section, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FinancialSubSection section, CancellationToken cancellationToken = default)
    {
        _context.FinancialSubSections.Update(section);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
