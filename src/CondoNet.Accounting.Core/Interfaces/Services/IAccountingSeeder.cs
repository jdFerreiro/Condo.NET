using CondoNet.Shared;

namespace CondoNet.Accounting.Core.Interfaces.Services
{
    public interface IAccountingSeeder
    {
        // Se añade soporte opcional para inicialización por parámetros directos
        Task<Result<bool>> SeedBaseCatalogAsync(Guid? tenantCondoId = null, Guid? tenantOrgId = null);
    }
}
