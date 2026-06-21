namespace CondoNet.Shared.Events.Notifications;

public record PaymentNotificationEvent
{
    public Guid CorrelationId { get; init; }
    public Guid PaymentId { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid UnitId { get; init; }
    public string ResidentEmail { get; init; } = string.Empty;
    public decimal AmountUSD { get; init; }
    public decimal AmountVES { get; init; }
    public decimal ExchangeRateApplied { get; init; }
    public DateTime ValidationTimestamp { get; init; }
    public bool IsBalanceCleared { get; init; }
}
