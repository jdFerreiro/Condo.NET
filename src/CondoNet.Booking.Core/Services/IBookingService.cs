using CondoNet.Booking.Core.DTOs;

namespace CondoNet.Booking.Core.Services
{
    public interface IBookingService
    {
        Task<bool> ReserveAsync(Core.Entities.Booking booking);
        Task<bool> ReserveRecurringAsync(Core.Entities.Booking booking, int recurrenceCount);
        Task<bool> ConfirmBookingAsync(Guid bookingId);
        Task<Core.Entities.Booking?> GetByIdAsync(Guid bookingId);
        Task<IEnumerable<Core.Entities.Booking>> GetByAssetAndPeriodAsync(Guid assetId, DateTime start, DateTime end);
        Task<bool> CancelAsync(Guid bookingId);
        Task<BookingMonthStatusDto> GetMonthStatusAsync(Guid assetId, int year, int month);
    }
}