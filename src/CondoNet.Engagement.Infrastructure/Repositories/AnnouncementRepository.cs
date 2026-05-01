using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Infrastructure.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly Persistence.EngagementDbContext _context;
        public AnnouncementRepository(Persistence.EngagementDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Announcement announcement) => await _context.Announcements.AddAsync(announcement);
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Announcements.FindAsync(id);
            if (entity != null) _context.Announcements.Remove(entity);
        }
        public async Task<IEnumerable<Announcement>> GetAllAsync() => await _context.Announcements.ToListAsync();
        public async Task<Announcement?> GetByIdAsync(Guid id) => await _context.Announcements.FindAsync(id);
        public async Task UpdateAsync(Announcement announcement) => _context.Announcements.Update(announcement);
        public async Task<bool> ExistsAsync(Guid id) => await _context.Announcements.AnyAsync(x => x.Id == id);
    }
}