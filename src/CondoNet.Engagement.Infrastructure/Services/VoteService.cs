using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using CondoNet.Engagement.Core.Services;
using System;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Infrastructure.Services
{
    public class VoteService : IVoteService
    {
        private readonly IVoteRepository _voteRepository;

        public VoteService(IVoteRepository voteRepository)
        {
            _voteRepository = voteRepository;
        }

        public async Task ValidateAndCastVoteAsync(Vote vote, Guid userId)
        {
            // Validar que el usuario esté habilitado (asumido válido)
            // Validar que no haya votado antes
            if (await _voteRepository.HasUserVotedAsync(vote.VotingSessionId, userId))
                throw new InvalidOperationException("El usuario ya ha votado en esta sesión.");

            vote.VoterId = userId;
            vote.CastAt = DateTime.UtcNow;
            await _voteRepository.AddAsync(vote);
        }
    }
}
