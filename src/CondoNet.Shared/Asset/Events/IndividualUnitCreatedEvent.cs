namespace CondoNet.Shared.Asset.Events;

public record IndividualUnitCreatedEvent
{
    public Guid UnitId { get; init; }
    public string Identifier { get; init; } = null!;
    public decimal Aliquot { get; init; }
    public string OwnerEmail { get; init; } = null!;
    public Guid OrganizationId { get; init; }

    public IndividualUnitCreatedEvent(Guid unitId, string identifier, decimal aliquot, string ownerEmail, Guid organizationId)
    {
        UnitId = unitId;
        Identifier = identifier;
        Aliquot = aliquot;
        OwnerEmail = ownerEmail;
        OrganizationId = organizationId;
    }
}
