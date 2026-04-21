namespace CondoNet.Shared.Asset.DTOs
{
    public record UnitBulkImportDto(
        string Identifier,      // Ej: "Apto 101"
        string TowerName,       // Ej: "Torre A"
        decimal Aliquot,        // Ej: 1.2500
        UnitType Type,          // Habitacional o Comercial
        string OwnerEmail
    );

    public record BulkImportRequest(
        Guid CondominiumId,
        List<UnitBulkImportDto> Units
    );
}
