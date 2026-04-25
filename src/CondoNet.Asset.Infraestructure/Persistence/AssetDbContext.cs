using CondoNet.Asset.Core.Entities;
using CondoNet.Asset.Core.Interfaces;
using CondoNet.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Asset.Infraestructure.Persistence
{
    // AssetDbContext.cs
    public class AssetDbContext(DbContextOptions<AssetDbContext> options, ITenantService tenantService) : DbContext(options)
    {
        private readonly Guid _organizationId = tenantService.GetOrganizationId();

        // Entidades del módulo
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Condominium> Condominiums { get; set; }
        public DbSet<Tower> Towers { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Core.Entities.Asset> Assets { get; set; }
        public DbSet<CriticalEquipment> CriticalEquipments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configuración de nombres de tabla (Snake Case para Postgres)
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName()?.ToLower().Replace("dbset", ""));
            }

            // 2. Aplicación de Global Query Filters para Multi-tenancy
            // Esto inyecta automáticamente "WHERE organization_id = @id" en cada SELECT
            modelBuilder.Entity<Condominium>().HasQueryFilter(e => e.OrganizationId == _organizationId);
            modelBuilder.Entity<Tower>().HasQueryFilter(e => e.OrganizationId == _organizationId);
            modelBuilder.Entity<Unit>().HasQueryFilter(e => e.OrganizationId == _organizationId);
            modelBuilder.Entity<Core.Entities.Asset>().HasQueryFilter(e => e.OrganizationId == _organizationId);
            modelBuilder.Entity<CriticalEquipment>().HasQueryFilter(e => e.OrganizationId == _organizationId);

            // 3. Precisiones y Validaciones específicas
            modelBuilder.Entity<Unit>(entity =>
            {
                entity.Property(e => e.Aliquot).HasPrecision(10, 4);
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasIndex(e => e.TaxId).IsUnique();
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssetDbContext).Assembly);
        }

        // 4. Interceptamos el Guardado para asignar el OrganizationId automáticamente
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.OrganizationId = _organizationId;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }

}
