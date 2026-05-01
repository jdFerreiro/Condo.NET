using CondoNet.Engagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Core.Repositories
{
    public interface IQuorumRepository
    {
        Task<Quorum?> GetByIdAsync(Guid id);
        Task<IEnumerable<Quorum>> GetAllByMeetingAsync(Guid meetingId);
        Task AddAsync(Quorum quorum);
        Task UpdateAsync(Quorum quorum);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}