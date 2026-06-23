using CondoNet.Shared;
using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Asset.Core.Interfaces
{
    public interface ITowerService
    {
        Task<Result<Guid>> CreateTowerAsync(string name);
        Task<Result<bool>> DeleteTowerAsync(Guid towerId);
        Task<List<TowerResponse>> GetTowersByCondoAsync();
        Task<Result<bool>> UpdateTowerAsync(Guid towerId, string newName);
    }
}