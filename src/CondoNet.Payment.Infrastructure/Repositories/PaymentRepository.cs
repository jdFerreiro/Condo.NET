using CondoNet.Payment.Core.Repositories;

namespace CondoNet.Payment.Infrastructure.Repositories;

public class PaymentRepository(Persistence.PaymentDbContext db) : IPaymentRepository
{
    private readonly Persistence.PaymentDbContext _db = db;

    public async Task<Core.Entities.Payment?> GetByIdAsync(Guid id)
        => await _db.Payments.FindAsync(id);

    public async Task AddAsync(Core.Entities.Payment payment)
    {
        await _db.Payments.AddAsync(payment);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Core.Entities.Payment payment)
    {
        _db.Payments.Update(payment);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Payments.FindAsync(id);
        if (entity != null)
        {
            _db.Payments.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
