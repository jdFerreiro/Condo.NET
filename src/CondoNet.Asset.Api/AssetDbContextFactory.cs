using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CondoNet.Asset.Api
{
    public class AssetDbContextFactory : IDesignTimeDbContextFactory<AssetDbContext>
    {
        public AssetDbContext CreateDbContext(string[] args)
        {
            // 1. Cargar la configuración desde el appsettings.json del API
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<Program>()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AssetDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString);

            // 2. Simulamos el TenantService para el tiempo de diseño (Migraciones)
            var dummyTenantService = new DummyTenantService();

            return new AssetDbContext(optionsBuilder.Options, dummyTenantService);
        }
    }

    // Clase auxiliar temporal para cumplir con la inyección de dependencias en consola
    internal class DummyTenantService : ITenantService
    {
        // Devolvemos un Guid vacío solo para pasar las migraciones de forma segura
        public Guid GetOrganizationId() => Guid.Empty;
    }
}
