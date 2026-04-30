using Microsoft.EntityFrameworkCore;

namespace CondoNet.Booking.Infrastructure.Persistence
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<Core.Entities.Booking> Bookings { get; set; }
        public DbSet<Core.Entities.Asset> Assets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuración adicional si es necesaria
        }
    }
}