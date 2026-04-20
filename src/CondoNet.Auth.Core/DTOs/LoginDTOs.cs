using CondoNet.Auth.Core.Entities;

namespace CondoNet.Auth.Core.DTOs;

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,          // Token inicial de identidad
    string FullName,
    string RefreshToken,
    List<AvailableContextResponse> Contexts // La lista de opciones
);

public record AvailableContextResponse(
    Guid ContextId,
    Guid? OrganizationId,
    Guid? CondoId,
    int RoleId,
    Role Role
);