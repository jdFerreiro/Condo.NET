using CondoNet.Engagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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