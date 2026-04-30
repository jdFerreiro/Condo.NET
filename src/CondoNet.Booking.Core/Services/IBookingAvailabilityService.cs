using System;
using System.Threading.Tasks;

namespace CondoNet.Booking.Core.Services
{
    public interface IBookingAvailabilityService
    {
        Task<bool> TryAcquireLockAsync(Guid assetId, Guid bookingId);
        Task<bool> ReleaseLockAsync(Guid assetId, Guid bookingId);
    }
}