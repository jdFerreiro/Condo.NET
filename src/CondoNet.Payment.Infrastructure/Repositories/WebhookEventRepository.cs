using CondoNet.Payment.Core.Entities;
using CondoNet.Payment.Core.Repositories;

namespace CondoNet.Payment.Infrastructure.Repositories;

public class WebhookEventRepository(Persistence.PaymentDbContext db) : IWebhookEventRepository
{
    private readonly Persistence.PaymentDbContext _db = db;

    public async Task<WebhookEvent?> GetByIdAsync(Guid id)
        => await _db.WebhookEvents.FindAsync(id);

    public async Task AddAsync(WebhookEvent webhookEvent)
    {
        await _db.WebhookEvents.AddAsync(webhookEvent);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(WebhookEvent webhookEvent)
    {
        _db.WebhookEvents.Update(webhookEvent);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.WebhookEvents.FindAsync(id);
        if (entity != null)
        {
            _db.WebhookEvents.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
