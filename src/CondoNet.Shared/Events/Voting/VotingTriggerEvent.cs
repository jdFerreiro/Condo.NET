namespace CondoNet.Shared.Events.Voting
{
    public record VotingTriggerEvent
    {
        public Guid CorrelationId { get; init; }
        public Guid ImpugnationId { get; init; }
        public Guid OriginalDisbursementId { get; init; }
        public string IssueDescription { get; init; } = string.Empty;
        public DateTime Deadline { get; init; }
    }
}
