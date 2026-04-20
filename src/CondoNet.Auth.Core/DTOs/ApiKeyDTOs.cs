namespace CondoNet.Auth.Core.DTOs;

public record CreateApiKeyRequest(string Description);

public record ApiKeyResponse(
    Guid Id,
    string Key,
    string Description,
    bool IsActive,
    DateTime CreatedAt
);
