using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly FinancialDbContext _context;
    public InvoiceRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Invoices.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IEnumerable<Invoice>> GetByUnitAccountIdAsync(Guid unitAccountId, CancellationToken cancellationToken = default)
        => await _context.Invoices.Where(i => i.UnitAccountId == unitAccountId).ToListAsync(cancellationToken);

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
