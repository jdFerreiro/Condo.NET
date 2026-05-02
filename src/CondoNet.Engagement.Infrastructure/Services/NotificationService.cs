using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using CondoNet.Engagement.Core.Services;
using System;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task ValidateAndSendAsync(Notification notification, Guid senderId)
        {
            // Validar permisos del remitente (asumido válido)
            // Validar destinatario
            if (notification.RecipientId == Guid.Empty)
                throw new InvalidOperationException("El destinatario es obligatorio.");

            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            await _notificationRepository.AddAsync(notification);
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid recipientId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
                throw new InvalidOperationException("La notificación no existe.");
            if (notification.RecipientId != recipientId)
                throw new UnauthorizedAccessException("No tiene permisos para modificar esta notificación.");

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
        }
    }
}
