using CondoNet.Shared;
using CondoNet.Shared.Auth.DTOs;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface IApiKeyService
    {
        Task<Result<ApiKeyResponse>> CreateApiKeyAsync(Guid orgId, string description);
        Task<Result<List<ApiKeyResponse>>> GetApiKeysByOrgAsync(Guid orgId);
        Task<Result<bool>> RevokeApiKeyAsync(Guid orgId, Guid apiKeyId);
        Task<Result<Guid>> ValidateApiKeyAsync(string rawApiKey);

    }
}
