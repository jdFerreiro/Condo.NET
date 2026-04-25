using CondoNet.Financial.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CondoNet.Financial.Infrastructure.Persistence
{
    public class FinancialDbContext(DbContextOptions<FinancialDbContext> options) : DbContext(options) //, ITenantService tenantService) : DbContext(options)
    {
        // El ID del condominio se obtiene del JWT a través de un servicio de contexto
        //private readonly Guid _condominiunId = tenantService.GetCondominiumId();
        //private readonly Guid _organizationId = tenantService.GetOrganizationId();

        private readonly Guid _condominiunId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // tenantService.GetCondominiumId();
        private readonly Guid _organizationId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // tenantService.GetOrganizationId();

        public DbSet<FinancialSubSection> FinancialSubSections => Set<FinancialSubSection>();
        public DbSet<UnitAccount> UnitAccounts => Set<UnitAccount>();
        public DbSet<UnitAccountSection> UnitAccountSections => Set<UnitAccountSection>();
        public DbSet<CondoExpense> CondoExpenses => Set<CondoExpense>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<BillingConfiguration> BillingConfigurations => Set<BillingConfiguration>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de Precisión Decimal (Finanzas & Blockchain)
            // Usamos 18,8 para alícuotas y 18,2 para dinero
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal)))
            {
                if (property.Name.Contains("Percentage") || property.Name.Contains("Participation"))
                {
                    property.SetPrecision(18);
                    property.SetScale(8);
                }
                else
                {
                    property.SetPrecision(18);
                    property.SetScale(2);
                }
            }

            // 2. Multi-tenancy: Filtro Global
            // Esto evita que un condominio vea datos de otro por error en cualquier consulta
            modelBuilder.Entity<FinancialSubSection>().HasQueryFilter(e => e.OrganizationId == _condominiunId);
            modelBuilder.Entity<UnitAccount>().HasQueryFilter(e => e.OrganizationId == _condominiunId);
            modelBuilder.Entity<CondoExpense>().HasQueryFilter(e => e.OrganizationId == _condominiunId);
            modelBuilder.Entity<Invoice>().HasQueryFilter(e => e.OrganizationId == _condominiunId);
            modelBuilder.Entity<Payment>().HasQueryFilter(e => e.OrganizationId == _condominiunId);

            // 3. Relación Jerárquica (FinancialSubSection)
            modelBuilder.Entity<FinancialSubSection>()
                .HasOne(s => s.ParentSection)
                .WithMany(s => s.ChildSections)
                .HasForeignKey(s => s.ParentSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Clave Compuesta para UnitAccountSection (N:N con datos extra)
            modelBuilder.Entity<UnitAccountSection>()
                .HasKey(uas => new { uas.UnitAccountId, uas.FinancialSubSectionId });

            // 5. Índices para rendimiento
            modelBuilder.Entity<UnitAccount>().HasIndex(u => u.ExternalUnitId);
            modelBuilder.Entity<Invoice>().HasIndex(i => i.Number).IsUnique();
            modelBuilder.Entity<CondoExpense>().HasIndex(e => new { e.Month, e.Year });

            // 6. Configuración de Enums como Strings en DB (Opcional, mejor legibilidad)
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .Property(p => p.Method)
                .HasConversion<string>();

            // Configuración adicional (opcional pero recomendada)
            modelBuilder.Entity<Transaction>(entity =>
            {

                entity.Property(t => t.Amount).HasPrecision(18, 2);
                entity.Property(t => t.Description).HasMaxLength(250);
            });

        }

        // Override para asignar automáticamente el OrganizationId al guardar
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CondominiumId = _condominiunId;
                    entry.Entity.OrganizationId = _organizationId;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
