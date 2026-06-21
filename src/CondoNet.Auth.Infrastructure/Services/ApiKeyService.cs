using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared.Auth.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using BC = BCrypt.Net.BCrypt;

namespace CondoNet.Auth.Infrastructure.Services;

public class ApiKeyService(AuthDbContext db) : IApiKeyService
{
    private readonly AuthDbContext _db = db;

    public async Task<Result<ApiKeyResponse>> CreateApiKeyAsync(Guid orgId, string description)
    {
        var rawKey = GenerateSecureKey();
        var hashedKey = BC.HashPassword(rawKey);

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            Key = hashedKey,
            OrganizationId = orgId,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.ApiKeys.Add(apiKey);
        await _db.SaveChangesAsync();

        return Result<ApiKeyResponse>.Success(new ApiKeyResponse(
            apiKey.Id, rawKey, apiKey.Description, apiKey.IsActive, apiKey.CreatedAt));
    }

    public async Task<Result<List<ApiKeyResponse>>> GetApiKeysByOrgAsync(Guid orgId)
    {
        var keys = await _db.ApiKeys
            .AsNoTracking() // Optimización de rendimiento para lecturas de administración
            .Where(k => k.OrganizationId == orgId)
            .Select(k => new ApiKeyResponse(
                k.Id,
                "****************",
                k.Description,
                k.IsActive,
                k.CreatedAt))
            .ToListAsync();

        return Result<List<ApiKeyResponse>>.Success(keys);
    }

    public async Task<Result<bool>> RevokeApiKeyAsync(Guid orgId, Guid apiKeyId)
    {
        var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == apiKeyId && k.OrganizationId == orgId);
        if (key == null) return Result<bool>.Failure("API Key no encontrada o no pertenece a la organización.");

        key.IsActive = false;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// REGLA DE NEGOCIO: Valida la API Key con BCrypt de forma segura y expone la Organización dueña.
    /// </summary>
    public async Task<Result<Guid>> ValidateApiKeyAsync(string rawApiKey)
    {
        if (string.IsNullOrWhiteSpace(rawApiKey))
            return Result<Guid>.Failure("La API Key provista no puede estar vacía.");

        // 1. Cargamos en memoria solo las llaves activas de la base de datos para validar el Hash.
        // NOTA: Si el volumen de llaves totales en la base de datos crece masivamente, la mejor práctica 
        // industrial a futuro es guardar un prefijo legible plano (ej: "condonet_live_abc123") en una columna 
        // indexada para filtrar por el prefijo en SQL y ejecutar el BCrypt.Verify únicamente sobre ese registro.
        var activeKeys = await _db.ApiKeys
            .AsNoTracking()
            .Where(k => k.IsActive)
            .Select(k => new { k.Key, k.OrganizationId })
            .ToListAsync();

        // 2. Buscamos mediante verificación criptográfica en memoria
        foreach (var keyItem in activeKeys)
        {
            if (BC.Verify(rawApiKey.Trim(), keyItem.Key))
            {
                // Devolvemos el OrganizationId asociado a la credencial válida
                return Result<Guid>.Success(keyItem.OrganizationId);
            }
        }

        return Result<Guid>.Failure("La API Key provista es inválida o se encuentra inactiva.");
    }

    private static string GenerateSecureKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "").Replace("/", "").Replace("=", "")[..32];
    }
}
