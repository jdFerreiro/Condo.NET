using CondoNet.Shared;
using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Asset.Core.Interfaces
{
    public interface IOrganizationService
    {
        Task<Result<OrganizationResponse>> GetOrganizationByIdAsync(Guid orgId);
        Task<Result<Guid>> RegisterOrganizationAsync(RegisterOrganizationRequest request);
        Task<Result<bool>> ToggleOrganizationStatusAsync(Guid orgId, bool isActive);
        Task<Result<bool>> UpdateOrganizationAsync(Guid orgId, UpdateOrgRequest request);
        Task<Result<bool>> UpdateSubscriptionPlanAsync(Guid orgId, int newPlanValue);
    }
}