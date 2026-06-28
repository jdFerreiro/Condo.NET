// CondoNet.Booking.Infrastructure.Persistence/BookingDbContext.cs
using CondoNet.Booking.Core.Entities;
using CondoNet.Shared.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Booking.Infrastructure.Persistence
{
    public class BookingDbContext(DbContextOptions<BookingDbContext> options, ITenantService tenantService) : DbContext(options)
    {
        private readonly ITenantService tenantService = tenantService;

        public DbSet<Core.Entities.Booking> Bookings { get; set; }
        public DbSet<Asset> Assets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Filtro Global Multi-Tenant: EF Core filtrará automáticamente por el condominio del JWT
            modelBuilder.Entity<Core.Entities.Booking>()
                .HasQueryFilter(b => b.CondoId == tenantService.GetCondominiumId());

            modelBuilder.Entity<Asset>()
                .HasQueryFilter(a => a.CondoId == tenantService.GetCondominiumId());

            // Validaciones adicionales de base de datos
            modelBuilder.Entity<Core.Entities.Booking>().Property(b => b.Price).HasPrecision(18, 2);

            modelBuilder.Entity<Asset>().Property(a => a.BookingRequiredPercentage).HasPrecision(5, 2);
            // ==========================================
            // CONFIGURACIÓN DE LAS TABLAS DEL OUTBOX
            // ==========================================
            // Genera automáticamente las tablas de persistencia para el patrón Outbox en SQL Server
            modelBuilder.AddTransactionalOutboxEntities();

        }
    }
}