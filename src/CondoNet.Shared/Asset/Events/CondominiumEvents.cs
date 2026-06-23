namespace CondoNet.Shared.Asset.Events
{
    public record CondominiumCreatedEvent
    {
        public Guid CondominiumId { get; set; }
        public Guid OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal ReserveFundPercentage { get; set; }

        public CondominiumCreatedEvent(Guid condominiumId, Guid organizationId, string name, decimal reserveFundPercentage)
        {
            CondominiumId = condominiumId;
            OrganizationId = organizationId;
            Name = name;
            ReserveFundPercentage = reserveFundPercentage;
        }

    }

    public record CondominiumUpdatedEvent
    {
        public Guid CondominiumId { get; init; }
        public Guid OrganizationId { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal ReserveFundPercentage { get; init; }
        public CondominiumUpdatedEvent(Guid condominiumId, Guid organizationId, string name, decimal reserveFundPercentage)
        {
            CondominiumId = condominiumId;
            OrganizationId = organizationId;
            Name = name;
            ReserveFundPercentage = reserveFundPercentage;
        }
    }

    public record CondominiumDeletedEvent
    {
        public Guid CondominiumId { get; init; }
        public Guid OrganizationId { get; init; }
        public CondominiumDeletedEvent(Guid condominiumId, Guid organizationId)
        {
            CondominiumId = condominiumId;
            OrganizationId = organizationId;
        }
    }
}
