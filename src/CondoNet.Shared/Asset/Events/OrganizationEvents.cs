namespace CondoNet.Shared.Asset.Events
{
    public record OrganizationRegisteredEvent
    {
        // Identificador único global de la Organización (Tenant Raíz)
        public Guid OrganizationId { get; init; }

        // Nombre comercial de la empresa administradora
        public string Name { get; init; } = null!;

        // Correo corporativo del administrador central (Útil para que Auth.Api le cree su cuenta)
        public string ContactEmail { get; init; } = null!;

        // Moneda de reporte por defecto asignada (ej: "USD", "COP")
        public string BaseCurrency { get; init; } = null!;

        // Constructor recomendado para inicialización y serialización limpia en RabbitMQ
        public OrganizationRegisteredEvent(Guid organizationId, string name, string contactEmail, string baseCurrency)
        {
            OrganizationId = organizationId;
            Name = name;
            ContactEmail = contactEmail;
            BaseCurrency = baseCurrency;
        }
    }

    public record OrganizationUpdatedEvent
    {
        public Guid OrganizationId { get; init; }
        public string NewName { get; init; } = null!;
        public string NewContactEmail { get; init; } = null!;
        public string NewBaseCurrency { get; init; } = null!;
        public OrganizationUpdatedEvent(Guid organizationId, string newName, string newContactEmail, string newBaseCurrency)
        {
            OrganizationId = organizationId;
            NewName = newName;
            NewContactEmail = newContactEmail;
            NewBaseCurrency = newBaseCurrency;
        }
    }
    public record OrganizationStatusChangedEvent
    {
        // Identificador de la Organización que mutó su estado (Tenant Raíz)
        public Guid OrganizationId { get; init; }

        // El nuevo estado de la administradora (true = Activa, false = Suspendida/Inactiva)
        public bool IsActive { get; init; }

        // Marca de tiempo oficial de la mutación para auditorías contables
        public DateTime ChangedAt { get; init; }

        // Constructor recomendado para la serialización y transporte en las colas de RabbitMQ
        public OrganizationStatusChangedEvent(Guid organizationId, bool isActive, DateTime changedAt)
        {
            OrganizationId = organizationId;
            IsActive = isActive;
            ChangedAt = changedAt;
        }
    }
}
