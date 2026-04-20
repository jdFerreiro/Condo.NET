namespace CondoNet.Shared.DTOs
{
    public record LoginRequest(string Email, string Password);

    public record LoginResponse(
        string Token,
        string FullName,
        string RefreshToken,
        List<AvailableContextResponse> Contexts,
        List<Permissions> Permissions
    );

    public record AvailableContextResponse(
        Guid ContextId,
        Guid OrganizationId,
        Guid? CondoId,
        List<string> Roles // <-- Solo el nombre, nada de objetos circulares
    );

    public record Permissions(
        int Id,
        string PermissionName
    );

    public record LogoutRequest(string RefreshToken);
}
