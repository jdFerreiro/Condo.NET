namespace CondoNet.Shared.Interfaces
{
    public interface ITenantService
    {
        Guid GetOrganizationId();
        Guid GetCondominiumId();
    }
}
