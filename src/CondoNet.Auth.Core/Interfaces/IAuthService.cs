using CondoNet.Auth.Core.DTOs;
using CondoNet.Shared;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface IAuthService
    {
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    }
}
