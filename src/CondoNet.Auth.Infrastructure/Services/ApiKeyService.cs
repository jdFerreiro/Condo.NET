using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using BC = BCrypt.Net.BCrypt;

namespace CondoNet.Auth.Infrastructure.Services
{
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

            // Devolvemos la rawKey solo esta vez para que el usuario la guarde
            return Result<ApiKeyResponse>.Success(new ApiKeyResponse(
                apiKey.Id, rawKey, apiKey.Description, apiKey.IsActive, apiKey.CreatedAt));
        }

        public async Task<Result<List<ApiKeyResponse>>> GetApiKeysByOrgAsync(Guid orgId)
        {
            var keys = await _db.ApiKeys
                .Where(k => k.OrganizationId == orgId)
                .Select(k => new ApiKeyResponse(
                    k.Id,
                    "****************", // Enmascaramos la llave en el listado
                    k.Description,
                    k.IsActive,
                    k.CreatedAt))
                .ToListAsync();

            return Result<List<ApiKeyResponse>>.Success(keys);
        }

        public async Task<Result<bool>> RevokeApiKeyAsync(Guid orgId, Guid apiKeyId)
        {
            var key = await _db.ApiKeys.FirstOrDefaultAsync(k => k.Id == apiKeyId && k.OrganizationId == orgId);

            if (key == null) return Result<bool>.Failure("API Key no encontrada");

            key.IsActive = false;
            await _db.SaveChangesAsync();

            return Result<bool>.Success(true);
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
}
