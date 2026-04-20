using CondoNet.Shared.DTOs;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface IPasswordService
    {
        Task<Result<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<Result<bool>> RequestResetAsync(string email);
        Task<Result<bool>> ExecuteResetAsync(string token, string newPassword);
    }
}
