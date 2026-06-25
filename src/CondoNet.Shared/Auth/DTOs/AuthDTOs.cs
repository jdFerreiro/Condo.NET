namespace CondoNet.Shared.Auth.DTOs
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
        string Name,
        string Path,
        string Description,
        string Icon,
        int DisplayOrder
    );

    public record LogoutRequest(string RefreshToken);

    // El DTO de respuesta estructurado que el Frontend leerá recursivamente para armar el Sidebar HTML
    public record MenuItemDto(
        int Id,
        string Name,
        string Path,
        string Icon,
        int DisplayOrder,
        List<MenuItemDto> Children
    );

}
