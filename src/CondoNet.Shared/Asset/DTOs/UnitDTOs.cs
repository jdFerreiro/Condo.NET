namespace CondoNet.Shared.Asset.DTOs
{
    public record UnitResponse
    {
        public Guid Id { get; set; }
        public Guid TowerId { get; set; }
        public string Identifier { get; set; } = null!;
        public string? Floor { get; set; }
        public decimal? AreaSquareMeters { get; set; }
        public string? Alias { get; set; }
        public decimal Aliquot { get; set; }
        public string OwnerEmail { get; set; } = null!;
        public string? Type { get; set; }
    }

    // DTO para la creación individual de unidades (Soluciona CS0246)
    public record CreateUnitRequest(
        string Identifier,
        string Floor,
        decimal? AreaSquareMeters,
        string? Alias,
        decimal Aliquot,
        UnitType Type,
        string OwnerEmail,
        Guid TowerId
    );

    public record UpdateUnitRequest(
        string Identifier,
        string Floor,
        decimal? AreaSquareMeters,
        string? Alias,
        decimal Aliquot,
        UnitType Type,
        string OwnerEmail,
        Guid TowerId
    );
}
