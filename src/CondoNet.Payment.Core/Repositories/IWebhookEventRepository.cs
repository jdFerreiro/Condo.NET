using CondoNet.Payment.Core.Entities;

namespace CondoNet.Payment.Core.Repositories;

public interface IWebhookEventRepository
{
    Task<WebhookEvent?> GetByIdAsync(Guid id);
    Task AddAsync(WebhookEvent webhookEvent);
    Task UpdateAsync(WebhookEvent webhookEvent);
    Task DeleteAsync(Guid id);
}
