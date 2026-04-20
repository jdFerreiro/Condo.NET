using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.DTOs;
using System.Security.Claims;

namespace CondoNet.Auth.Api.Endpoints;

public static class PasswordEndpoints
{
    public static void MapPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/passwords").WithTags("Seguridad de Contraseñas");

        group.MapPost("/change", async (ChangePasswordRequest req, IPasswordService service, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await service.ChangePasswordAsync(userId, req.CurrentPassword, req.NewPassword);
            return result.IsSuccess ? Results.Ok("Contraseña actualizada.") : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost("/reset-request", async (ResetPasswordRequest req, IPasswordService service) =>
        {
            await service.RequestResetAsync(req.Email);
            return Results.Accepted(); // Siempre Accepted para evitar enumeración de emails
        });

        group.MapPost("/reset-execute", async (ExecuteResetRequest req, IPasswordService service) =>
        {
            var result = await service.ExecuteResetAsync(req.Token, req.NewPassword);
            return result.IsSuccess ? Results.Ok("Contraseña restablecida.") : Results.BadRequest(result.Error);
        });
    }
}
