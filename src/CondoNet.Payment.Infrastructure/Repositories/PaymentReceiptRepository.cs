using CondoNet.Payment.Core.Entities;
using CondoNet.Payment.Core.Repositories;

namespace CondoNet.Payment.Infrastructure.Repositories;

public class PaymentReceiptRepository(Persistence.PaymentDbContext db) : IPaymentReceiptRepository
{
    private readonly Persistence.PaymentDbContext _db = db;

    public async Task<PaymentReceipt?> GetByIdAsync(Guid id)
        => await _db.PaymentReceipts.FindAsync(id);

    public async Task AddAsync(PaymentReceipt receipt)
    {
        await _db.PaymentReceipts.AddAsync(receipt);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PaymentReceipt receipt)
    {
        _db.PaymentReceipts.Update(receipt);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.PaymentReceipts.FindAsync(id);
        if (entity != null)
        {
            _db.PaymentReceipts.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
