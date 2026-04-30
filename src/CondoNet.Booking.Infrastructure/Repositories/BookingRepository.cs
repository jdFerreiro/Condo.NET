using CondoNet.Booking.Core.Repositories;
using CondoNet.Booking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Booking.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;
        public BookingRepository(BookingDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(Core.Entities.Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Core.Entities.Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task<Core.Entities.Booking?> GetByIdAsync(Guid bookingId)
        {
            return await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<bool> ExistsAsync(Guid assetId, DateTime start, DateTime end)
        {
            return await _context.Bookings.AnyAsync(b => b.AssetId == assetId &&
                ((b.Start < end && b.End > start)));
        }

        public async Task<IEnumerable<Core.Entities.Booking>> GetByAssetAndPeriodAsync(Guid assetId, DateTime start, DateTime end)
        {
            return await _context.Bookings
                .Where(b => b.AssetId == assetId && b.Start < end && b.End > start)
                .ToListAsync();
        }
    }
}