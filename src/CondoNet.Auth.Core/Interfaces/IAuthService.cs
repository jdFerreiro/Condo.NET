using CondoNet.Shared.DTOs;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    }
}
