using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared.Auth.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CondoNet.Auth.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static void MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/apikeys")
                       .WithTags("Gestión de API Keys");

        group.MapPost("/", async (CreateApiKeyRequest request, IApiKeyService service, ClaimsPrincipal user) =>
        {
            var orgId = GetOrgId(user);
            if (orgId == Guid.Empty) return Results.Unauthorized();

            var result = await service.CreateApiKeyAsync(orgId, request.Description);
            return Results.Created($"/api/auth/apikeys/{result.Value!.Id}", result.Value);
        })
        .RequireAuthorization(policy => policy.RequireClaim("role", "ADMIN"));

        group.MapGet("/", async (IApiKeyService service, ClaimsPrincipal user) =>
        {
            var orgId = GetOrgId(user);
            var result = await service.GetApiKeysByOrgAsync(orgId);
            return Results.Ok(result.Value);
        })
        .RequireAuthorization(policy => policy.RequireClaim("role", "ADMIN"));

        group.MapDelete("/{id:guid}", async (Guid id, IApiKeyService service, ClaimsPrincipal user) =>
        {
            var orgId = GetOrgId(user);
            var result = await service.RevokeApiKeyAsync(orgId, id);

            return result.IsSuccess ? Results.NoContent() : Results.NotFound(result.Error);
        })
        .RequireAuthorization(policy => policy.RequireClaim("role", "ADMIN"));

        group.MapGet("/validate", async ([FromServices] AuthDbContext db, [FromQuery] string key) =>
        {
            if (string.IsNullOrWhiteSpace(key))
                return Results.Unauthorized();

            var exists = await db.ApiKeys.AnyAsync(a => a.Key == key && a.IsActive);

            return exists ? Results.Ok() : Results.Unauthorized();
        })
        .AllowAnonymous()
        .WithName("ValidateApiKey")
        .WithTags("ApiKey")
        .Produces(200)
        .Produces(401);

    }

    private static Guid GetOrgId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("org_id")?.Value ?? user.FindFirst("orgId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
