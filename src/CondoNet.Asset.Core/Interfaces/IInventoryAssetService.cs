using CondoNet.Shared;
using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Asset.Core.Interfaces
{
    public interface IInventoryAssetService
    {
        Task<Result<bool>> DeleteAssetAsync(Guid assetId);
        Task<List<AssetInventoryResponse>> GetActiveAssetsAsync();
        Task<Result<AssetInventoryResponse>> GetAssetByIdAsync(Guid assetId);
        Task<List<EquipmentResponse>> GetCriticalEquipmentsAsync();
        Task<Result<bool>> LinkAssetToUnitAsync(Guid assetId, Guid? linkedUnitId);
        Task<Result<Guid>> RegisterAssetAsync(RegisterAssetRequest request);
        Task<Result<Guid>> RegisterCriticalEquipmentAsync(RegisterEquipmentRequest request);
        Task<Result<bool>> UpdateAssetAsync(Guid assetId, UpdateAssetRequest request);
        Task<Result<bool>> UpdateAssetStatusAsync(Guid assetId, int newStatusValue);
    }
}