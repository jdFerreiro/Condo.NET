namespace CondoNet.Auth.Core.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Path { get; set; } = null!; // URL para el router del Frontend
        public string Description { get; set; } = null!;
        public string Icon { get; set; } = null!; // Cambiado de Image a Icon (ej: "users", "settings")
        public int DisplayOrder { get; set; } // Controla la posición visual en el menú sidebar

        // Soporte para menús multinivel (Hijos y Padres)
        public int? ParentPermissionId { get; set; }
        public Permission? ParentPermission { get; set; }
        public List<Permission> ChildPermissions { get; set; } = [];
        public List<RolePermission> RolePermissions { get; set; } = [];

    }
}
