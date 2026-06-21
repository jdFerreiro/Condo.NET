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
                       .RequireAuthorization("RequiredAdmin"); // Restringido nativamente a administradores

        // POST: Registrar un nuevo usuario (Residente / Colaborador) dentro de la Organización actual
        group.MapPost("/", async (CreateUserRequest request, IUserService userService, ClaimsPrincipal user) =>
        {
            // CORREGIDO: Extrae la claim unificada oficial de tu TenantService
            var orgClaim = user.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgClaim) || !Guid.TryParse(orgClaim, out var orgId))
                return Results.Unauthorized();

            // REGLA DE MULTI-TENANT: Inyectamos de forma obligatoria el OrganizationId verificado en el DTO
            // Esto asegura que el usuario sea asignado con un UserContext exclusivo de la empresa del administrador
            var enrichedRequest = request with { OrganizationId = orgId };

            var result = await userService.CreateUserAsync(enrichedRequest);

            return result.IsSuccess
                ? Results.Created($"/api/auth/users/{result.Value!.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        // GET: Listar todos los usuarios y perfiles pertenecientes a la Organización del administrador
        group.MapGet("/", async (IUserService userService, ClaimsPrincipal user) =>
        {
            // CORREGIDO: Extrae la claim unificada oficial de tu TenantService
            var orgClaim = user.FindFirst("OrganizationId")?.Value;
            if (string.IsNullOrEmpty(orgClaim) || !Guid.TryParse(orgClaim, out var orgId))
                return Results.Unauthorized();

            var result = await userService.GetUsersByOrganizationAsync(orgId);
            return Results.Ok(result.Value);
        });
    }
}
