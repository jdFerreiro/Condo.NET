using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Repositories;
using System;
using System.Threading.Tasks;

namespace CondoNet.Engagement.Core.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;

        public AnnouncementService(IAnnouncementRepository announcementRepository)
        {
            _announcementRepository = announcementRepository;
        }

        public async Task ValidateAndCreateAsync(Announcement announcement, Guid userId)
        {
            // Validar permisos del usuario (solo autorizados)
            // Aquí deberías validar si el usuario tiene permisos, por ahora se asume que sí

            // Validar fechas de vigencia
            if (announcement.CreatedAt > DateTime.UtcNow)
                throw new InvalidOperationException("La fecha de creación no puede ser futura.");

            // Setear CreatedAt y CreatedBy
            announcement.CreatedAt = DateTime.UtcNow;
            announcement.CreatedBy = userId;
            announcement.IsActive = true;

            await _announcementRepository.AddAsync(announcement);
        }

        public async Task ValidateAndUpdateAsync(Announcement announcement, Guid userId)
        {
            var existing = await _announcementRepository.GetByIdAsync(announcement.Id);
            if (existing == null)
                throw new InvalidOperationException("El anuncio no existe.");

            // Validar permisos (solo el creador o admin, por ejemplo)
            if (existing.CreatedBy != userId)
                throw new UnauthorizedAccessException("No tiene permisos para modificar este anuncio.");

            // Validar fechas
            if (announcement.CreatedAt > DateTime.UtcNow)
                throw new InvalidOperationException("La fecha de creación no puede ser futura.");

            await _announcementRepository.UpdateAsync(announcement);
        }

        public async Task ValidateAndDeleteAsync(Guid announcementId, Guid userId)
        {
            var existing = await _announcementRepository.GetByIdAsync(announcementId);
            if (existing == null)
                throw new InvalidOperationException("El anuncio no existe.");

            // Validar permisos (solo el creador o admin, por ejemplo)
            if (existing.CreatedBy != userId)
                throw new UnauthorizedAccessException("No tiene permisos para eliminar este anuncio.");

            await _announcementRepository.DeleteAsync(announcementId);
        }
    }
}
