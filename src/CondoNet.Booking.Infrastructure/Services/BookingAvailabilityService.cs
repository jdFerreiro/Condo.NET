using CondoNet.Booking.Core.Services;
using StackExchange.Redis;

namespace CondoNet.Booking.Infrastructure.Services
{
    public class BookingAvailabilityService : IBookingAvailabilityService
    {
        private readonly IDatabase _redis;
        private readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(30);

        public BookingAvailabilityService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task<bool> TryAcquireLockAsync(Guid assetId, Guid bookingId)
        {
            var key = $"booking:lock:{assetId}";
            // Set lock with NX (only if not exists) and expiry
            return await _redis.StringSetAsync(key, bookingId.ToString(), _lockTimeout, When.NotExists);
        }

        public async Task<bool> ReleaseLockAsync(Guid assetId, Guid bookingId)
        {
            var key = $"booking:lock:{assetId}";
            var value = await _redis.StringGetAsync(key);
            if (value == bookingId.ToString())
            {
                return await _redis.KeyDeleteAsync(key);
            }
            return false;
        }
    }
}
