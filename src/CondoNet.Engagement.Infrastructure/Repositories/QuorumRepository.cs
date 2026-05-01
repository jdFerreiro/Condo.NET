using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Infrastructure.Repositories
{
    public class QuorumRepository : IQuorumRepository
    {
        private readonly Persistence.EngagementDbContext _context;
        public QuorumRepository(Persistence.EngagementDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Quorum quorum) => await _context.Quorums.AddAsync(quorum);
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Quorums.FindAsync(id);
            if (entity != null) _context.Quorums.Remove(entity);
        }
        public async Task<IEnumerable<Quorum>> GetAllByMeetingAsync(Guid meetingId) => await _context.Quorums.Where(x => x.MeetingId == meetingId).ToListAsync();
        public async Task<Quorum?> GetByIdAsync(Guid id) => await _context.Quorums.FindAsync(id);
        public async Task UpdateAsync(Quorum quorum) => _context.Quorums.Update(quorum);
        public async Task<bool> ExistsAsync(Guid id) => await _context.Quorums.AnyAsync(x => x.Id == id);
    }
}