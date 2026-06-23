namespace CondoNet.Shared.Asset.DTOs
{
    public record UnitBulkImportDto(
        string Identifier,      // Ej: "Apto 101"
        string TowerName,       // Ej: "Torre A"
        decimal Aliquot,        // Ej: 1.2500
        string Floor,          // Ej: "1", "PB", "S1"
        UnitType Type,          // Habitacional o Comercial
        decimal? AreaSquareMeters, // Opcional, útil para justificar alícuotas o avalúos
        string? Alias,
        string OwnerEmail
    );

    public record BulkImportRequest(
        Guid CondominiumId,
        List<UnitBulkImportDto> Units
    );
}
