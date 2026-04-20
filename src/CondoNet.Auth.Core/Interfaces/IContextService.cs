using CondoNet.Auth.Core.DTOs;
using CondoNet.Auth.Core.Entities;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface IContextService
    {
        Task<List<AvailableContextResponse>> GetUserContextsAsync(Guid userId);
        Task<UserContext?> ValidateAndGetContextAsync(Guid userId, Guid contextId);
    }
}
