namespace CondoNet.Shared.Auth.Events
{
    public record PasswordChangedEvent(Guid UserId, string Email, DateTime Date);
    public record PasswordResetRequestedEvent(Guid UserId, string Email, string Token, DateTime Expiry);
}
