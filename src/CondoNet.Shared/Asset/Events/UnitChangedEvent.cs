namespace CondoNet.Shared.Asset.Events;

public class UnitChangedEvent
{
    public Guid UserId { get; set; }
    public Guid OldUnitId { get; set; }
    public Guid NewUnitId { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
