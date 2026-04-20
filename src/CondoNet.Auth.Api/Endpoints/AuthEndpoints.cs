using CondoNet.Auth.Core.DTOs;
using CondoNet.Auth.Core.Interfaces;

namespace CondoNet.Auth.Api.Endpoints;

public static class AuthEndpoints
{


    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Autenticación");

        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Json(new { error = result.Error }, statusCode: 401);
        })
        .AllowAnonymous()
        .WithName("Login");
    }
}
