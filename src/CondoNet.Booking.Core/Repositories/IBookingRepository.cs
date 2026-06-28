using CondoNet.Booking.Core.Entities;

namespace CondoNet.Booking.Core.Repositories
{
    public interface IBookingRepository
    {
        Task<Entities.Booking?> GetByIdAsync(Guid id, Guid condoId);
        Task AddAsync(Entities.Booking booking);
        Task UpdateAsync(Entities.Booking booking);
        Task<bool> HasOverlappingBookingAsync(Guid condoId, Guid assetId, DateTime start, DateTime end);
        Task<bool> HasAnyOverlappingOccurrencesAsync(Guid condoId, Guid assetId, IEnumerable<(DateTime Start, DateTime End)> occurrences);
        Task<bool> ExistsAsync(Guid id, Guid condoId);
        Task<IEnumerable<Entities.Booking>> GetByAssetAndPeriodAsync(Guid condoId, Guid assetId, DateTime start, DateTime end);
        Task<bool> IsUserBlockedByDebtAsync(Guid condoId, Guid userId);
        Task SaveDebtStatusAsync(UserDebtStatus status);

        // ==========================================
        // SOLUCIÓN: Agregar la firma que falta
        // ==========================================
        Task<Asset?> GetAssetByIdAsync(Guid assetId, Guid condoId);
        Task<bool> HasActiveMaintenanceBlockAsync(Guid condoId, Guid assetId, IEnumerable<(DateTime Start, DateTime End)> occurrences);

    }
}