namespace CondoNet.Auth.Api.DTOs;

public record CreateUserRequest(
    string Email,
    string Password,
    string FullName,
    string Role, // OWNER, EXTERNAL
    Guid CondoId
);

public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive
);
