using CondoNet.Financial.Core.Entities;
using CondoNet.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Financial.Infraestructure.Persistence
{
    public class FinancialDbContext(ITenantService tenantService) : DbContext
    {
        private readonly ITenantService _tenantService = tenantService;

        public DbSet<UnitAccount> UnitAccounts => Set<UnitAccount>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<CondoExpense> CondoExpenses => Set<CondoExpense>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de precisión para montos financieros
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // Filtro Global para Multi-tenancy
            // Nota: El TenantService se inyecta para obtener el OrgId del JWT
            modelBuilder.Entity<UnitAccount>().HasQueryFilter(u => u.OrganizationId == _tenantService.GetOrganizationId());

            // Índices para búsqueda rápida
            modelBuilder.Entity<UnitAccount>().HasIndex(u => u.ExternalUnitId);
            modelBuilder.Entity<Invoice>().HasIndex(i => i.FolioNumber).IsUnique();
        }
    }
}
