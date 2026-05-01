using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly Persistence.EngagementDbContext _context;
        public NotificationRepository(Persistence.EngagementDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Notification notification) => await _context.Notifications.AddAsync(notification);
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Notifications.FindAsync(id);
            if (entity != null) _context.Notifications.Remove(entity);
        }
        public async Task<IEnumerable<Notification>> GetAllByRecipientAsync(Guid recipientId) => await _context.Notifications.Where(x => x.RecipientId == recipientId).ToListAsync();
        public async Task<Notification?> GetByIdAsync(Guid id) => await _context.Notifications.FindAsync(id);
        public async Task UpdateAsync(Notification notification) => _context.Notifications.Update(notification);
        public async Task<bool> ExistsAsync(Guid id) => await _context.Notifications.AnyAsync(x => x.Id == id);
    }
}