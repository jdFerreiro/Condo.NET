using CondoNet.Shared.Auth.DTOs;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface IUserService
    {
        Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request);
        Task<Result<List<UserResponse>>> GetUsersByOrganizationAsync(Guid orgId);
    }
}
