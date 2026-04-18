using CondoNet.Auth.Api.DTOs;
using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using BC = BCrypt.Net.BCrypt;

namespace CondoNet.Auth.Api.Endpoints;

public static class ApiKeyEndpoints
{
    public static void MapApiKeyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/apikeys")
                       .WithTags("Gestión de API Keys")
                       .RequireAuthorization(policy => policy.RequireRole("ADMIN"));

        // 1. Generar nueva API Key
        group.MapPost("/", async (
            CreateApiKeyRequest request,
            AuthDbContext db,
            ClaimsPrincipal userPrincipal) =>
        {
            var orgIdClaim = userPrincipal.FindFirst("org_id")?.Value;
            if (!Guid.TryParse(orgIdClaim, out var orgId)) return Results.Unauthorized();

            // Generar una llave aleatoria segura
            var newKey = GenerateSecureKey();
            var hashedKey = BC.HashPassword(newKey);

            var apiKey = new ApiKey
            {
                Id = Guid.NewGuid(),
                Key = hashedKey, // Guardamos el hash de la key
                OrganizationId = orgId,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.ApiKeys.Add(apiKey);
            await db.SaveChangesAsync();

            return Results.Created($"/api/auth/apikeys/{apiKey.Id}",
                new ApiKeyResponse(apiKey.Id, newKey, apiKey.Description, apiKey.IsActive, apiKey.CreatedAt));
        });

        // 2. Listar llaves de la organización
        group.MapGet("/", async (AuthDbContext db, ClaimsPrincipal userPrincipal) =>
        {
            var orgIdClaim = userPrincipal.FindFirst("org_id")?.Value;
            if (!Guid.TryParse(orgIdClaim, out var orgId)) return Results.Unauthorized();

            var keys = await db.ApiKeys
                .Where(k => k.OrganizationId == orgId)
                .Select(k => new ApiKeyResponse(k.Id, k.Key, k.Description, k.IsActive, k.CreatedAt))
                .ToListAsync();

            return Results.Ok(keys);
        });

        // 3. Revocar (Desactivar) una llave
        group.MapDelete("/{id:guid}", async (Guid id, AuthDbContext db, ClaimsPrincipal userPrincipal) =>
        {
            var orgIdClaim = userPrincipal.FindFirst("org_id")?.Value;
            if (!Guid.TryParse(orgIdClaim, out var orgId)) return Results.Unauthorized();

            var key = await db.ApiKeys.FirstOrDefaultAsync(k => k.Id == id && k.OrganizationId == orgId);
            if (key == null) return Results.NotFound();

            key.IsActive = false;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    private static string GenerateSecureKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "")
[..32];
    }
}
