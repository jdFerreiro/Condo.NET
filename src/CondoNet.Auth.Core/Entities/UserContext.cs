namespace CondoNet.Auth.Core.Entities
{
    public class UserContext
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? CondoId { get; set; }

        // Cambio: Referencia al ID del Rol
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}