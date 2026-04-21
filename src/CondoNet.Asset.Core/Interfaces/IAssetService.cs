using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Asset.Core.Interfaces
{
    public interface IAssetService
    {
        Task<bool> BulkImportUnitsAsync(BulkImportRequest request);
    }
}
