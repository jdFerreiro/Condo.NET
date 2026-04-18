namespace CondoNet.Auth.Core.Entities
{
    public class ApiKey
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = null!;
        public Guid OrganizationId { get; set; }
        public string Description { get; set; } = null!; // Ej: "WebApp Administradora X"
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}