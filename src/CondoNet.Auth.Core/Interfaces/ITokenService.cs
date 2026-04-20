using CondoNet.Auth.Core.DTOs;
using CondoNet.Auth.Core.Entities;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface ITokenService
    {
        Task<LoginResponse> GenerateContextTokenAsync(Guid userId, UserContext context);
    }
}
