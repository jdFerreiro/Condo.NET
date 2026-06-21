namespace CondoNet.Shared.Auth.Events
{
    public record UserCreatedEvent
    (
        Guid UserId,
        string Email,
        string FullName
    );

    public record UserContextSwitchedEvent(
        Guid UserId,
        Guid? OldCondoId,
        Guid? NewCondoId,
        DateTime Date
    );

    public record UserContextAssignedEvent
    {
        public Guid ContextId { get; init; }
        public Guid UserId { get; init; }
        public Guid OrganizationId { get; init; }
        public Guid? CondoId { get; init; }

        // Transportamos la colección de roles asignados a este contexto específico
        public List<string> Roles { get; init; } = [];

        // Constructor recomendado para inicialización limpia
        public UserContextAssignedEvent(Guid contextId, Guid userId, Guid organizationId, Guid? condoId, List<string> roles)
        {
            ContextId = contextId;
            UserId = userId;
            OrganizationId = organizationId;
            CondoId = condoId;
            Roles = roles;
        }
    }
}
