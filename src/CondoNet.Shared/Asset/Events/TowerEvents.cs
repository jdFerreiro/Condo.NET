namespace CondoNet.Shared.Asset.Events
{
    // Evento de creación de Torre individual
    public record TowerCreatedEvent(Guid TowerId, string Name, Guid CondominiumId, Guid OrganizationId);

    // Evento de eliminación de Torre individual
    public record TowerDeletedEvent(Guid TowerId, Guid CondominiumId, Guid OrganizationId);
}
