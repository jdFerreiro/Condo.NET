using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingEntity = CondoNet.Booking.Core.Entities.Booking;

namespace CondoNet.Booking.Core.Repositories
{
    public interface IBookingRepository
    {
        Task AddAsync(BookingEntity booking);
        Task UpdateAsync(BookingEntity booking);
        Task<BookingEntity?> GetByIdAsync(Guid bookingId);
        Task<bool> ExistsAsync(Guid assetId, DateTime start, DateTime end);
        Task<IEnumerable<BookingEntity>> GetByAssetAndPeriodAsync(Guid assetId, DateTime start, DateTime end);
    }
}