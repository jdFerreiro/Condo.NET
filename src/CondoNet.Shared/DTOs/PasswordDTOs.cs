namespace CondoNet.Shared.DTOs
{
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    public record ResetPasswordRequest(string Email);
    public record ExecuteResetRequest(string Token, string NewPassword);
}
