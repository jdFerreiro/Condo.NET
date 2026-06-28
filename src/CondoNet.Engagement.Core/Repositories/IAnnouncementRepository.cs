using CondoNet.Engagement.Core.Entities;

namespace CondoNet.Engagement.Core.Repositories
{
    public interface IAnnouncementRepository
    {
        Task<Announcement?> GetByIdAsync(Guid id);
        Task<IEnumerable<Announcement>> GetAllAsync();
        Task AddAsync(Announcement announcement);
        Task UpdateAsync(Announcement announcement);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}