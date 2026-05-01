using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Core.Services
{
    public class QuorumService : IQuorumService
    {
        private readonly IQuorumRepository _quorumRepository;

        public QuorumService(IQuorumRepository quorumRepository)
        {
            _quorumRepository = quorumRepository;
        }

        public async Task ValidateAndRecordAsync(Quorum quorum, Guid userId)
        {
            // Validar permisos (asumido válido)
            quorum.RecordedAt = System.DateTime.UtcNow;
            await _quorumRepository.AddAsync(quorum);
        }

        public async Task<bool> IsQuorumMetAsync(Guid meetingId)
        {
            var quorums = await _quorumRepository.GetAllByMeetingAsync(meetingId);
            var last = quorums.OrderByDescending(q => q.RecordedAt).FirstOrDefault();
            if (last == null) return false;
            return last.PresentCount >= last.RequiredCount;
        }
    }
}
