namespace CondoNet.Shared.Asset.DTOs
{
    public record RegisterAssetRequest(
        string Name,
        int Category,
        bool IsRentable,
        decimal? DefaultRentalPrice,
        Guid? LinkedUnitId
    );

    // DTO de respuesta para el listado del inventario (Soluciona CS0246)
    public record AssetInventoryResponse(
        Guid Id,
        string Name,
        string Category,
        string Status,
        Guid? LinkedUnitId
    );

    public record UpdateAssetRequest(
        string Name,
        int Category,
        bool IsRentable,
        decimal? DefaultRentalPrice,
        Guid? LinkedUnitId
    );

    public record RegisterEquipmentRequest(
        string Name,
        string Brand,
        string Model,
        DateTime InstallationDate,
        int MaintenanceFrequencyDays,
        string ManualUrl
    );

    public record EquipmentResponse(
        Guid EquipmentId,
        string Name,
        string Brand,
        string Model,
        DateTime InstallationDate,
        int MaintenanceFrequencyDays
    );
}
