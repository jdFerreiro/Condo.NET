namespace CondoNet.Shared.Contracts.Events
{
    public abstract record IntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
        public Guid OrganizationId { get; init; }

        // Constructor protegido para forzar la asignación del OrganizationId obligatorio
        protected IntegrationEvent(Guid organizationId)
        {
            OrganizationId = organizationId;
        }
    }
}
