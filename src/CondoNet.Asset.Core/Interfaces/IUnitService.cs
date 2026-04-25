using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Asset.Core.Interfaces;

public interface IUnitService
{
    Task<List<UnitResponse>> GetUnitsByOwnerAsync(string ownerId);
    Task ChangeUnitAsync(Guid userId, Guid oldUnitId, Guid newUnitId);
}
