namespace CondoNet.Engagement.Core.Services
{
    public interface IQuorumService
    {
        Task ValidateAndRecordAsync(Entities.Quorum quorum, Guid userId);
        Task<bool> IsQuorumMetAsync(Guid meetingId);
    }
}