using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class ReportedPaymentRepository(FinancialDbContext context) : IReportedPaymentRepository
{
    private readonly FinancialDbContext _context = context;

    public async Task<IEnumerable<ReportedPayment>> GetPendingAsync(CancellationToken cancellationToken = default)
        => await _context.Set<ReportedPayment>()
            .Where(r => !r.IsVerified)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(ReportedPayment payment, CancellationToken cancellationToken = default)
    {
        _context.ReportedPayments.Update(payment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
