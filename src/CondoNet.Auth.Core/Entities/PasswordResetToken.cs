namespace CondoNet.Auth.Core.Entities
{
    public class PasswordResetToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
