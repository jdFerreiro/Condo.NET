using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IInvoiceItemRepository
{
    Task<InvoiceItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<InvoiceItem>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task AddAsync(InvoiceItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(InvoiceItem item, CancellationToken cancellationToken = default);
}
