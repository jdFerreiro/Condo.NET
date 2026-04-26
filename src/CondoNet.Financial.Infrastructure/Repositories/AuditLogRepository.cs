using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly Persistence.FinancialDbContext _context;
    public AuditLogRepository(Persistence.FinancialDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog log, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
