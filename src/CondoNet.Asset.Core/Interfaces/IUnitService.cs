using CondoNet.Shared;
using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Asset.Core.Interfaces
{
    public interface IUnitService
    {
        Task ChangeUnitAsync(Guid userId, Guid oldUnitId, Guid newUnitId);
        Task<Result<Guid>> CreateUnitAsync(CreateUnitRequest request);
        Task<Result<bool>> DeleteUnitAsync(Guid unitId);
        Task<List<UnitResponse>> GetUnitsByOwnerAsync(string ownerId);
        Task<Result<List<UnitResponse>>> GetUnitsByTowerAsync(Guid towerId);
        Task<Result<bool>> UpdateUnitAsync(Guid unitId, UpdateUnitRequest request);
    }
}