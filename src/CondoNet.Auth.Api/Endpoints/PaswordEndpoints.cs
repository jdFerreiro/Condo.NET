using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.Auth.DTOs;
using System.Security.Claims;

namespace CondoNet.Auth.Api.Endpoints;

public static class PasswordEndpoints
{
    public static void MapPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/passwords").WithTags("Seguridad de Contraseñas");

        // 1. Cambio de contraseña manual (Usuario Autenticado por JWT, exento de ApiKey)
        group.MapPost("/change", async (ChangePasswordRequest req, IPasswordService service, ClaimsPrincipal user) =>
        {
            var claimValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimValue)) return Results.Unauthorized();

            var userId = Guid.Parse(claimValue);
            var result = await service.ChangePasswordAsync(userId, req.CurrentPassword, req.NewPassword);

            return result.IsSuccess ? Results.Ok("Contraseña actualizada con éxito.") : Results.BadRequest(result.Error);
        })
        .AllowAnonymous() // Salta el ApiKeyMiddleware para el cliente final
        .RequireAuthorization("RequiredAnyRole"); // Mantiene la validación estricta del JWT Bearer

        // 2. Solicitar enlace/token de recuperación de contraseña (Acceso Público)
        group.MapPost("/reset-request", async (ResetPasswordRequest req, IPasswordService service) =>
        {
            await service.RequestResetAsync(req.Email);
            return Results.Accepted(); // Retorno síncrono ciego idempotente para prevenir fugas de emails
        })
        .AllowAnonymous();

        // 3. Ejecutar el restablecimiento físico con el token recibido (Acceso Público)
        group.MapPost("/reset-execute", async (ExecuteResetRequest req, IPasswordService service) =>
        {
            var result = await service.ExecuteResetAsync(req.Token, req.NewPassword);
            return result.IsSuccess ? Results.Ok("Contraseña restablecida con éxito.") : Results.BadRequest(result.Error);
        })
        .AllowAnonymous();
    }
}
