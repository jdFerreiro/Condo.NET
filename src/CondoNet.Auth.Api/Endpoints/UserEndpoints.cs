using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.Auth.DTOs;
using System.Security.Claims;

namespace CondoNet.Auth.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/users")
                       .WithTags("Gestión de Usuarios")
                       .RequireAuthorization();

        // POST: Crear usuario
        group.MapPost("/", async (CreateUserRequest request, IUserService userService, ClaimsPrincipal user) =>
        {
            var orgId = Guid.Parse(user.FindFirst("org_id")?.Value ?? Guid.Empty.ToString());
            if (orgId == Guid.Empty) return Results.Unauthorized();

            var result = await userService.CreateUserAsync(request);

            return result.IsSuccess
                ? Results.Created($"/api/auth/users/{result.Value!.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        // GET: Listar usuarios
        group.MapGet("/", async (IUserService userService, ClaimsPrincipal user) =>
        {
            var orgId = Guid.Parse(user.FindFirst("org_id")?.Value ?? Guid.Empty.ToString());

            var result = await userService.GetUsersByOrganizationAsync(orgId);
            return Results.Ok(result.Value);
        });
    }
}
