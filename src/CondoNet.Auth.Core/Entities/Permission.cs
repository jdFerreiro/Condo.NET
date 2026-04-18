namespace CondoNet.Auth.Core.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // Ej: "parking:lease", "billing:write"
        public string Description { get; set; } = null!;
        public List<RolePermission> RolePermissions { get; set; } = [];
    }
}