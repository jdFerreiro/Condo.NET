using CondoNet.Auth.Core.Entities;
using CondoNet.Shared.Auth.DTOs;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface IContextService
    {
        Task<List<AvailableContextResponse>> GetUserContextsAsync(Guid userId);
        Task<UserContext?> ValidateAndGetContextAsync(Guid userId, Guid contextId);
    }
}
