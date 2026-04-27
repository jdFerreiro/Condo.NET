using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class CurrencyAdjustmentLogRepository : ICurrencyAdjustmentLogRepository
{
    private readonly FinancialDbContext _context;
    public CurrencyAdjustmentLogRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CurrencyAdjustmentLog log, CancellationToken cancellationToken = default)
    {
        await _context.CurrencyAdjustmentLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<CurrencyAdjustmentLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyAdjustmentLogs
            .Where(l => l.EntityType.ToString() == entityType && l.EntityId == entityId)
            .ToListAsync(cancellationToken);
    }
}
