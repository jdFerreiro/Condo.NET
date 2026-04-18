namespace CondoNet.Auth.Core.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<UserContext> Contexts { get; set; } = [];
        public List<PasswordResetToken> PasswordResetTokens { get; set; } = [];
        public List<RefreshToken> RefreshTokens { get; set; } = [];
    }
}