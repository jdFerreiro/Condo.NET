using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class AuditLogRepository(Persistence.FinancialDbContext context) : IAuditLogRepository
{
    private readonly Persistence.FinancialDbContext _context = context;

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
