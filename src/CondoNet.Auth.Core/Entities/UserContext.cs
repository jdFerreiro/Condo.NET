namespace CondoNet.Auth.Core.Entities
{
    public class UserContext
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? CondoId { get; set; }
        public ContextStatus Status { get; set; } = ContextStatus.Active;
        public User User { get; set; } = null!;
        public ICollection<Role> Roles { get; set; } = [];
    }

    public enum ContextStatus
    {
        Active = 1,
        Inactive = 2,
        Pending = 3,
        Suspended = 4
    }

}