using CondoNet.Payment.Core.Entities;

namespace CondoNet.Payment.Core.Repositories;

public interface IPaymentReceiptRepository
{
    Task<PaymentReceipt?> GetByIdAsync(Guid id);
    Task AddAsync(PaymentReceipt receipt);
    Task UpdateAsync(PaymentReceipt receipt);
    Task DeleteAsync(Guid id);
}
