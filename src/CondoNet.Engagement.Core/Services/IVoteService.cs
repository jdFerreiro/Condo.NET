namespace CondoNet.Engagement.Core.Services
{
    public interface IVoteService
    {
        Task ValidateAndCastVoteAsync(Entities.Vote vote, Guid userId);
    }
}