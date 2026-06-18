namespace CondoNet.Auth.Core.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Path { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<RolePermission> RolePermissions { get; set; } = [];
        public string Image { get; set; } = null!;
    }
}