namespace CondoNet.Payment.Core.Repositories;

public interface IPaymentRepository
{
    Task<Entities.Payment?> GetByIdAsync(Guid id);
    Task AddAsync(Entities.Payment payment);
    Task UpdateAsync(Entities.Payment payment);
    Task DeleteAsync(Guid id);
}
