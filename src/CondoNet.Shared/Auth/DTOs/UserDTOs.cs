namespace CondoNet.Shared.Auth.DTOs;

public record CreateUserRequest(
    string Email,
    string Password,
    string FullName,
    Guid OrganizationId,
    Guid CondoId
);

public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive
);
