namespace CondoNet.Auth.Core.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // ADMIN, OWNER, EXTERNAL, CONCIERGE
                                                  // Relación: Un rol puede estar asignado a muchos contextos de usuario
        public List<UserContext> Contexts { get; set; } = [];
        public List<RolePermission> RolePermissions { get; set; } = [];
    }
}