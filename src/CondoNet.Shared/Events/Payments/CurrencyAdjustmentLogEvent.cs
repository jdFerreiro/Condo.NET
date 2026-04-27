using System;

namespace CondoNet.Shared.Events.Payments;

public record CurrencyAdjustmentLogEvent
{
    public Guid AdjustmentId { get; init; }
    public Guid OrganizationId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
    public decimal PreviousRate { get; init; }
    public decimal NewRate { get; init; }
    public decimal AmountVesDiff { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
