namespace CondoNet.Shared.Asset.Events
{
    public record UnitsImported
    {
        public Guid OrganizationId { get; init; }
        public Guid CondominiumId { get; init; }
        public List<ImportedUnitDto> Units { get; init; } = [];

        public UnitsImported(Guid organizationId, Guid condominiumId, List<ImportedUnitDto> units)
        {
            OrganizationId = organizationId;
            CondominiumId = condominiumId;
            Units = units;
        }
    }

    // Sub-DTO inmutable requerido para el transporte del lote de datos en el evento masivo
    public record ImportedUnitDto
    {
        public Guid Id { get; init; }
        public string Identifier { get; init; } = null!;
        public decimal Aliquot { get; init; }
        public string OwnerEmail { get; init; } = null!;

        public ImportedUnitDto(Guid id, string identifier, decimal aliquot, string ownerEmail)
        {
            Id = id;
            Identifier = identifier;
            Aliquot = aliquot;
            OwnerEmail = ownerEmail;
        }
    }
}
