using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.Auth.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CondoNet.Auth.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static void MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/apikeys").WithTags("Gestión de API Keys");

        // Crear una nueva API Key para la Organización del Administrador
        group.MapPost("/", async (CreateApiKeyRequest request, IApiKeyService service, ClaimsPrincipal user) =>
        {
            var orgId = GetOrgId(user);
            if (orgId == Guid.Empty) return Results.Unauthorized();

            var result = await service.CreateApiKeyAsync(orgId, request.Description);
            return result.IsSuccess ? Results.Created($"/api/auth/apikeys/{result.Value!.Id}", result.Value) : Results.BadRequest(result.Error);
        })
        .RequireAuthorization("RequiredAdmin");

        // Listar todas las llaves de la Organización actual (enmascaradas)
        group.MapGet("/", async (IApiKeyService service, ClaimsPrincipal user) =>
        {
            var orgId = GetOrgId(user);
            if (orgId == Guid.Empty) return Results.Unauthorized();

            var result = await service.GetApiKeysByOrgAsync(orgId);
            return Results.Ok(result.Value);
        })
        .RequireAuthorization("RequiredAdmin"); // CORREGIDO: Utiliza tu política unificada de Program.cs

        // Revocar/Invalidar una API Key específica
        group.MapDelete("/{id:guid}", async (Guid id, IApiKeyService service, ClaimsPrincipal user) =>
        {
            var orgId = GetOrgId(user);
            if (orgId == Guid.Empty) return Results.Unauthorized();

            var result = await service.RevokeApiKeyAsync(orgId, id);
            return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error);
        })
        .RequireAuthorization("RequiredAdmin");

        // CORREGIDO: Endpoint de validación acoplado con tu ApiKeyMiddleware (Opción 1)
        group.MapGet("/validate", async ([FromQuery] string key, IApiKeyService service) =>
        {
            if (string.IsNullOrWhiteSpace(key)) return Results.Unauthorized();

            var result = await service.ValidateApiKeyAsync(key);

            // Retorna el OrganizationId (Guid) en el cuerpo de la respuesta para que el Middleware configure el Tenant
            return result.IsSuccess ? Results.Ok(result.Value) : Results.Unauthorized();
        })
        .AllowAnonymous()
        .WithName("ValidateApiKey")
        .Produces<Guid>(200)
        .Produces(401);
    }

    private static Guid GetOrgId(ClaimsPrincipal user)
    {
        // CORREGIDO: Busca la claim oficial estandarizada en toda tu solución CondoNet
        var claim = user.FindFirst("OrganizationId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
