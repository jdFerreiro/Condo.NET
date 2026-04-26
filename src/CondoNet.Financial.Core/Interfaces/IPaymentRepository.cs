using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}
