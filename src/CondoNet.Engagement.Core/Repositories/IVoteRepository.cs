using CondoNet.Engagement.Core.Entities;

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