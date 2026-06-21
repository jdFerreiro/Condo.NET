using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.Auth.DTOs;

namespace CondoNet.Auth.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Autenticación");

        // Endpoint de Autenticación Principal (Acceso Público)
        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Json(new { error = result.Error }, statusCode: 401);
        })
        .AllowAnonymous()
        .WithName("Login");

        // Endpoint de Cierre de Sesión (Protegido por JWT, exento de ApiKey)
        group.MapPost("/logout", async (LogoutRequest request, ILogoutService logoutService) =>
        {
            var result = await logoutService.LogoutAsync(request.RefreshToken);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Error);
        })
        // Exime a este endpoint del ApiKeyMiddleware (ya que el middleware valida context.GetEndpoint()?.Metadata.Get<AllowAnonymousAttribute>())
        .AllowAnonymous()
        // Fuerza a que requiera obligatoriamente un token JWT válido de usuario activo para ejecutarse
        .RequireAuthorization("RequiredAnyRole")
        .WithName("Logout");
    }
}
