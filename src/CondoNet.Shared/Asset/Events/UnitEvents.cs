namespace CondoNet.Shared.Asset.Events
{
    public record UnitChangedEvent
    {
        public Guid UserId { get; init; }
        public Guid OldUnitId { get; init; }
        public Guid NewUnitId { get; init; }
        public DateTime ChangedAt { get; init; }

        // CORREGIDO (CS1729): Añadido el constructor explícito de 4 argumentos que exige tu UnitService
        public UnitChangedEvent(Guid userId, Guid oldUnitId, Guid newUnitId, DateTime changedAt)
        {
            UserId = userId;
            OldUnitId = oldUnitId;
            NewUnitId = newUnitId;
            ChangedAt = changedAt;
        }
    }

    public record UnitDeletedEvent
    {
        public Guid UnitId { get; init; }
        public string Identifier { get; init; } = null!;
        public decimal Aliquot { get; init; }
        public string OwnerEmail { get; init; } = null!;
        public Guid OrganizationId { get; init; }
        public UnitDeletedEvent(Guid unitId, string identifier, decimal aliquot, string ownerEmail, Guid organizationId)
        {
            UnitId = unitId;
            Identifier = identifier;
            Aliquot = aliquot;
            OwnerEmail = ownerEmail;
            OrganizationId = organizationId;
        }
    }

    public record UnitMetadataUpdatedEvent
    {
        public Guid UnitId { get; init; }
        public Guid OrganizationId { get; init; }
        public string NewIdentifier { get; init; } = null!;
        public string NewOwnerEmail { get; init; } = null!;
        public decimal NewAliquot { get; init; }

        public UnitMetadataUpdatedEvent(Guid unitId, Guid organizationId, string newIdentifier, string newOwnerEmail, decimal newAliquot)
        {
            UnitId = unitId;
            OrganizationId = organizationId;
            NewIdentifier = newIdentifier;
            NewOwnerEmail = newOwnerEmail;
            NewAliquot = newAliquot;
        }
    }
}
