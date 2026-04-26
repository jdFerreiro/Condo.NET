using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class FinancialCondominiumConfigurationRepository(Persistence.FinancialDbContext context) : IFinancialCondominiumConfigurationRepository
{
    private readonly Persistence.FinancialDbContext _context = context;

    public async Task<FinancialCondominiumConfiguration?> GetByCondominiumIdAsync(Guid condominiumId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<FinancialCondominiumConfiguration>()
            .FirstOrDefaultAsync(c => c.CondominiumId == condominiumId, cancellationToken);
    }

    public async Task AddAsync(FinancialCondominiumConfiguration config, CancellationToken cancellationToken = default)
    {
        await _context.Set<FinancialCondominiumConfiguration>().AddAsync(config, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FinancialCondominiumConfiguration config, CancellationToken cancellationToken = default)
    {
        _context.Set<FinancialCondominiumConfiguration>().Update(config);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
