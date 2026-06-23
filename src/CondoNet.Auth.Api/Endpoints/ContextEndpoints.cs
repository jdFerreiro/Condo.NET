using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared;
using CondoNet.Shared.Auth.DTOs;
using System.Security.Claims;

namespace CondoNet.Auth.Api.Endpoints;

public static class ContextEndpoints
{
    public static void MapContextEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user/context")
            .WithTags("Gestión de Contextos")
            .RequireAuthorization(); // Fuerza seguridad por JWT base para todo el grupo

        // 1. Listar entornos multi-tenant disponibles para el residente (JWT obligatorio, exento de ApiKey)
        group.MapGet("/available", async (IContextService contextService, ClaimsPrincipal user) =>
        {
            var claimValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimValue)) return Results.Unauthorized();

            var userId = Guid.Parse(claimValue);
            var contexts = await contextService.GetUserContextsAsync(userId);
            return Results.Ok(contexts);
        })
        .AllowAnonymous() // Permite omitir el ApiKeyMiddleware para flujos de apps móviles
        .RequireAuthorization("RequiredAnyRole") // Mantiene la protección estricta del JWT
        .WithName("Available");

        // 2. Conmutar sesión y firmar un nuevo JWT enriquecido (JWT obligatorio, exento de ApiKey)
        group.MapPost("/select", async (SelectContextRequest request, IContextService contextService, ITokenService tokenService, ClaimsPrincipal user) =>
        {
            var claimValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claimValue)) return Results.Unauthorized();

            var userId = Guid.Parse(claimValue);

            // Validamos criptográficamente que el entorno le pertenezca al solicitante activo
            var context = await contextService.ValidateAndGetContextAsync(userId, request.ContextId);
            if (context is null)
                return Results.Problem("El contexto solicitado no es válido, se encuentra inactivo o no le pertenece.", statusCode: 404);

            // Genera el Token definitivo estructurado bajo la nomenclatura "OrganizationId" y "CondoId"
            var loginResponse = await tokenService.GenerateContextTokenAsync(userId, context);
            return Results.Ok(loginResponse);
        })
        .AllowAnonymous()
        .RequireAuthorization("RequiredAnyRole")
        .WithName("SwitchContext");

        // 3. NUEVO: Regla de negocio expuesta para dar de alta a un usuario en una Organización/Condominio
        group.MapPost("/assign", async (AssignContextRequest request, IContextService contextService) =>
        {
            var result = await contextService.AssignContextAsync(request);

            // Si tiene éxito, devuelve el ContextId creado y gatilla de fondo el UserContextAssignedEvent por MassTransit
            return result.IsSuccess
                ? Results.Created($"/api/user/context/{result.Value}", new { ContextId = result.Value })
                : Results.BadRequest(result.Error);
        })
        .RequireAuthorization("RequiredAdmin") // Restringido únicamente a administradores globales
        .WithName("AssignContext");
    }
}
