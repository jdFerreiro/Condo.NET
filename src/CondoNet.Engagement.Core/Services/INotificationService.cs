namespace CondoNet.Engagement.Core.Services
{
    public interface INotificationService
    {
        Task ValidateAndSendAsync(Entities.Notification notification, Guid senderId);
        Task MarkAsReadAsync(Guid notificationId, Guid recipientId);
    }
}