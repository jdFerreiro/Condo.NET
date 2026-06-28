using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Engagement.Infrastructure.Repositories
{
    public class VoteRepository : IVoteRepository
    {
        private readonly Persistence.EngagementDbContext _context;
        public VoteRepository(Persistence.EngagementDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Vote vote) => await _context.Votes.AddAsync(vote);
        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Votes.FindAsync(id);
            if (entity != null) _context.Votes.Remove(entity);
        }
        public async Task<IEnumerable<Vote>> GetAllByVotingSessionAsync(Guid votingSessionId) => await _context.Votes.Where(x => x.VotingSessionId == votingSessionId).ToListAsync();
        public async Task<Vote?> GetByIdAsync(Guid id) => await _context.Votes.FindAsync(id);
        public async Task UpdateAsync(Vote vote) => _context.Votes.Update(vote);
        public async Task<bool> ExistsAsync(Guid id) => await _context.Votes.AnyAsync(x => x.Id == id);
        public async Task<bool> HasUserVotedAsync(Guid votingSessionId, Guid userId) => await _context.Votes.AnyAsync(x => x.VotingSessionId == votingSessionId && x.VoterId == userId);
    }
}