namespace CondoNet.Engagement.Core.Services
{
    public interface IAnnouncementService
    {
        Task ValidateAndCreateAsync(Entities.Announcement announcement, Guid userId);
        Task ValidateAndUpdateAsync(Entities.Announcement announcement, Guid userId);
        Task ValidateAndDeleteAsync(Guid announcementId, Guid userId);
    }
}