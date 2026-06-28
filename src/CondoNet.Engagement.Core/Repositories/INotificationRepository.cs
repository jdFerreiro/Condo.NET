using CondoNet.Engagement.Core.Entities;

namespace CondoNet.Engagement.Core.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);
        Task<IEnumerable<Notification>> GetAllByRecipientAsync(Guid recipientId);
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}