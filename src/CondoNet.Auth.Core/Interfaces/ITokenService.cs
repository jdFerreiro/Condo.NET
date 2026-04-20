using CondoNet.Auth.Core.Entities;
using CondoNet.Shared.DTOs;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface ITokenService
    {
        Task<LoginResponse> GenerateContextTokenAsync(Guid userId, UserContext context);
    }
}
