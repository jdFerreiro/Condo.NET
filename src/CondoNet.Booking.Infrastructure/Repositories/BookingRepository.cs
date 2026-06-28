// CondoNet.Booking.Infrastructure.Repositories/BookingRepository.cs
using CondoNet.Booking.Core.Entities;
using CondoNet.Booking.Core.Repositories;
using CondoNet.Booking.Infrastructure.Persistence;
using CondoNet.Shared.Booking.Enums;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Booking.Infrastructure.Repositories
{
    public class BookingRepository(BookingDbContext context) : IBookingRepository
    {
        public async Task<Core.Entities.Booking?> GetByIdAsync(Guid id, Guid condoId)
        {
            // El filtro global ya restringe por condoId, pero aseguramos robustez
            return await context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.CondoId == condoId);
        }

        public async Task AddAsync(Core.Entities.Booking booking)
        {
            await context.Bookings.AddAsync(booking);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Core.Entities.Booking booking)
        {
            context.Bookings.Update(booking);
            await context.SaveChangesAsync();
        }

        public async Task<bool> HasOverlappingBookingAsync(Guid condoId, Guid assetId, DateTime start, DateTime end)
        {
            // Fórmula matemática de cruces: (Inicio1 < Fin2) Y (Fin1 > Inicio2)
            return await context.Bookings
                .AnyAsync(b => b.CondoId == condoId &&
                               b.AssetId == assetId &&
                               b.IsConfirmed &&
                               start < b.End &&
                               end > b.Start);
        }
        public async Task<bool> HasAnyOverlappingOccurrencesAsync(Guid condoId, Guid assetId, IEnumerable<(DateTime Start, DateTime End)> occurrences)
        {
            if (!occurrences.Any()) return false;

            var query = context.Bookings
                .Where(b => b.CondoId == condoId && b.AssetId == assetId && b.IsConfirmed);

            var minStart = occurrences.Min(o => o.Start);
            var maxEnd = occurrences.Max(o => o.End);

            // Traemos solo los candidatos potenciales que se cruzan con el rango macro
            var potentialConflicts = await query
                .Where(b => b.Start < maxEnd && b.End > minStart)
                .Select(b => new { b.Start, b.End })
                .ToListAsync();

            // Evaluación matemática limpia en memoria
            foreach (var (Start, End) in occurrences)
            {
                bool hasCollision = potentialConflicts.Any(b =>
                    Start < b.End && End > b.Start);

                if (hasCollision) return true;
            }

            return false;
        }
        public async Task<bool> ExistsAsync(Guid id, Guid condoId)
        {
            return await context.Bookings
                .AnyAsync(b => b.Id == id && b.CondoId == condoId);
        }

        public async Task<IEnumerable<Core.Entities.Booking>> GetByAssetAndPeriodAsync(Guid condoId, Guid assetId, DateTime start, DateTime end)
        {
            return await context.Bookings
                .Where(b => b.CondoId == condoId &&
                            b.AssetId == assetId &&
                            b.Start < end &&
                            b.End > start)
                .ToListAsync();
        }

        public async Task<bool> IsUserBlockedByDebtAsync(Guid condoId, Guid userId)
        {
            // El filtro global de Tenant ya cubre el CondoId, pero lo dejamos explícito
            return await context.Set<UserDebtStatus>()
                .AnyAsync(u => u.CondoId == condoId && u.UserId == userId && u.IsBlocked);
        }

        public async Task SaveDebtStatusAsync(UserDebtStatus status)
        {
            var existing = await context.Set<UserDebtStatus>()
                .FirstOrDefaultAsync(u => u.CondoId == status.CondoId && u.UserId == status.UserId);

            if (existing == null)
            {
                await context.Set<UserDebtStatus>().AddAsync(status);
            }
            else
            {
                existing.IsBlocked = status.IsBlocked;
                existing.UpdatedAt = status.UpdatedAt;
            }
            await context.SaveChangesAsync();
        }

        public async Task<Asset?> GetAssetByIdAsync(Guid assetId, Guid condoId)
        {
            // El query filter global del DbContext ya restringe por el Condominio del JWT,
            // pero incluimos la condición explícita para robustecer la regla de negocio.
            return await context.Assets
                .FirstOrDefaultAsync(a => a.Id == assetId && a.CondoId == condoId);
        }

        public async Task<bool> HasActiveMaintenanceBlockAsync(Guid condoId, Guid assetId, IEnumerable<(DateTime Start, DateTime End)> occurrences)
        {
            if (!occurrences.Any()) return false;

            // Filtramos las reservas del tenant que sean estrictamente bloqueos de mantenimiento
            var query = context.Bookings
                .Where(b => b.CondoId == condoId &&
                            b.AssetId == assetId &&
                            b.Type == BookingType.MaintenanceBlock);

            var minStart = occurrences.Min(o => o.Start);
            var maxEnd = occurrences.Max(o => o.End);

            // Traemos los candidatos macro a memoria para indexación óptima
            var activeBlocks = await query
                .Where(b => b.Start < maxEnd && b.End > minStart)
                .Select(b => new { b.Start, b.End })
                .ToListAsync();

            // Evaluación matemática fina de colisión en memoria
            foreach (var (Start, End) in occurrences)
            {
                bool isBlocked = activeBlocks.Any(b =>
                    Start < b.End && End > b.Start);

                if (isBlocked) return true; // Chocó con un día festivo o mantenimiento
            }

            return false;
        }

    }
}