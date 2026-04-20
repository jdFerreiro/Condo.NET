using CondoNet.Auth.Core.DTOs;
using CondoNet.Auth.Core.Interfaces;
using System.Security.Claims;

namespace CondoNet.Auth.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static void MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/apikeys")
                       .WithTags("Gestión de API Keys")
                       .RequireAuthorization(policy => policy.RequireClaim("role", "ADMIN"));

        group.MapPost("/", async (CreateApiKeyRequest request, IApiKeyService service, ClaimsPrincipal user) =>
        {
            var orgId = GetOrgId(user);
            if (orgId == Guid.Empty) return Results.Unauthorized();

            var result = await service.CreateApiKeyAsync(orgId, request.Description);
            return Results.Created($"/api/auth/apikeys/{result.Value!.Id}", result.Value);
        });

        group.MapGet("/", async (IApiKeyService service, ClaimsPrincipal user) =>
        {
            var orgId = GetOrgId(user);
            var result = await service.GetApiKeysByOrgAsync(orgId);
            return Results.Ok(result.Value);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IApiKeyService service, ClaimsPrincipal user) =>
        {
            var orgId = GetOrgId(user);
            var result = await service.RevokeApiKeyAsync(orgId, id);

            return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error);
        });
    }

    private static Guid GetOrgId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("org_id")?.Value ?? user.FindFirst("orgId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
