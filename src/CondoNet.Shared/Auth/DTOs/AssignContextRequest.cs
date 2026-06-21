namespace CondoNet.Shared.Auth.DTOs
{
    public record AssignContextRequest
    {
        // Identificador del usuario al que se le va a dar el alta
        public Guid UserId { get; init; }

        // Identificador de la Organización (Empresa administradora global)
        public Guid OrganizationId { get; init; }

        // Identificador del Condominio específico (Opcional, Nulo si es un rol global de la Org)
        public Guid? CondoId { get; init; }

        // Lista de nombres de roles que va a ejercer en este entorno (ej. ["Resident"], ["Manager"])
        public List<string> RoleNames { get; init; } = [];

        // Constructor inmutable recomendado para .NET moderno
        public AssignContextRequest(Guid userId, Guid organizationId, Guid? condoId, List<string> roleNames)
        {
            UserId = userId;
            OrganizationId = organizationId;
            CondoId = condoId;
            RoleNames = roleNames;
        }
    }
}
