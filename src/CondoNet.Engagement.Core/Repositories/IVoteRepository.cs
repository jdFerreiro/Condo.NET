using CondoNet.Engagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Core.Repositories
{
    public interface IVoteRepository
    {
        Task<Vote?> GetByIdAsync(Guid id);
        Task<IEnumerable<Vote>> GetAllByVotingSessionAsync(Guid votingSessionId);
        Task AddAsync(Vote vote);
        Task UpdateAsync(Vote vote);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> HasUserVotedAsync(Guid votingSessionId, Guid userId);
    }
}