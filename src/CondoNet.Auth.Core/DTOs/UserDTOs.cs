namespace CondoNet.Auth.Core.DTOs;

public record CreateUserRequest(
    string Email,
    string Password,
    string FullName
);

public record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive
);
