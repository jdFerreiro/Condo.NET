using Microsoft.EntityFrameworkCore;

namespace CondoNet.Payment.Infrastructure.Persistence
{
    public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
    {
        public DbSet<Core.Entities.Payment> Payments => Set<Core.Entities.Payment>();
        public DbSet<Core.Entities.PaymentGateway> PaymentGateways => Set<Core.Entities.PaymentGateway>();
        public DbSet<Core.Entities.WebhookEvent> WebhookEvents => Set<Core.Entities.WebhookEvent>();
        public DbSet<Core.Entities.Invoice> Invoices => Set<Core.Entities.Invoice>();
        public DbSet<Core.Entities.PaymentReceipt> PaymentReceipts => Set<Core.Entities.PaymentReceipt>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new Configurations.PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.PaymentGatewayConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.WebhookEventConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.InvoiceConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.PaymentReceiptConfiguration());
        }
    }
}
