using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infrastructure.Repositories;

public class InvoiceItemRepository : IInvoiceItemRepository
{
    private readonly FinancialDbContext _context;
    public InvoiceItemRepository(FinancialDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.InvoiceItems.FindAsync(new object[] { id }, cancellationToken);

    public async Task<IEnumerable<InvoiceItem>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
        => await _context.InvoiceItems.Where(i => i.InvoiceId == invoiceId).ToListAsync(cancellationToken);

    public async Task AddAsync(InvoiceItem item, CancellationToken cancellationToken = default)
    {
        await _context.InvoiceItems.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(InvoiceItem item, CancellationToken cancellationToken = default)
    {
        _context.InvoiceItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
