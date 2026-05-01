using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class BillingConfigurationRepository(FinancialDbContext context) : IBillingConfigurationRepository
{
    private readonly FinancialDbContext _context = context;

    public async Task<BillingConfiguration> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.BillingConfigurations.FindAsync([id], cancellationToken);

    public async Task<BillingConfiguration> GetCurrentAsync(CancellationToken cancellationToken = default)
        => await _context.BillingConfigurations.OrderByDescending(b => b.Id).FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(BillingConfiguration config, CancellationToken cancellationToken = default)
    {
        await _context.BillingConfigurations.AddAsync(config, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(BillingConfiguration config, CancellationToken cancellationToken = default)
    {
        _context.BillingConfigurations.Update(config);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
