namespace CondoNet.Shared.Events.Asset;

public class CondoCreatedEvent
{
    public Guid CorrelationId { get; init; }
    public Guid CondominiumId { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
}
