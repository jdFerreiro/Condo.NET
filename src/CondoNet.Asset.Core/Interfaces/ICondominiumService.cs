using CondoNet.Shared;
using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Asset.Core.Interfaces
{
    public interface ICondominiumService
    {
        Task<Result<Guid>> CreateCondominiumAsync(CreateCondominiumRequest request);
        Task<Result<bool>> DeleteCondominiumAsync(Guid condoId);
        Task<Result<GetCondominiumResponse>> GetCondominiumByIdAsync(Guid condoId);
        Task<Result<List<GetCondominiumResponse>>> GetCondominiumsByOrganizationAsync();
        Task<Result<bool>> UpdateCondominiumAsync(Guid condoId, UpdateCondominiumRequest request);
    }
}