namespace CondoNet.Auth.Api.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    string FullName,
    string Role,
    int RoleId,
    Guid OrganizationId,
    Guid? CondoId,
    string RefreshToken
);
