using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class InvoiceRepository(FinancialDbContext context) : IInvoiceRepository
{
    private readonly FinancialDbContext _context = context;

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Invoices.FindAsync([id], cancellationToken);

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
    public async Task<IEnumerable<Invoice>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Invoices.ToListAsync(cancellationToken);
}
