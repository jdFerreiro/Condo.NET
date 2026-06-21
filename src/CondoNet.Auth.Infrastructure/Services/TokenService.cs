using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.Auth.DTOs;
using CondoNet.Shared.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CondoNet.Auth.Infrastructure.Services;

public class TokenService(IOptions<JwtSettings> settings) : ITokenService
{
    private readonly JwtSettings _settings = settings.Value;

    /// <summary>
    /// REGLA DE NEGOCIO: Genera un nuevo token JWT firmado y enriquecido con las claims del contexto seleccionado.
    /// </summary>
    public async Task<LoginResponse> GenerateContextTokenAsync(Guid userId, UserContext context)
    {
        // 1. Crear Claims utilizando la nomenclatura exacta requerida por TenantService y logs JSON
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, context.User.Email),
            new("OrganizationId", context.OrganizationId.ToString()) // CORREGIDO: Coincide con TenantService
        };

        // Si el contexto tiene un condominio específico, lo agregamos jerárquicamente
        if (context.CondoId.HasValue)
        {
            claims.Add(new("CondoId", context.CondoId.Value.ToString())); // CORREGIDO: Coincide con TenantService
        }

        // 2. Extraer y aplanar los Roles y Permisos específicos de este contexto
        var permissionsList = new List<Permissions>();
        foreach (var role in context.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
            foreach (var rp in role.RolePermissions)
            {
                claims.Add(new Claim("permissions", rp.Permission.Name));
                permissionsList.Add(new Permissions(rp.Permission.Id, rp.Permission.Name));
            }
        }

        // Eliminar duplicados de permisos si el usuario comparte accesos cruzados
        permissionsList = [.. permissionsList.DistinctBy(p => p.Id)];

        // 3. Generación criptográfica real del token JWT firmado
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: creds
        );

        var finalToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        // 4. Retornar la respuesta estructurada DTO para el cliente
        return new LoginResponse(
            Token: finalToken,
            FullName: context.User.FullName,
            Contexts: [], // Vacío ya que la conmutación se ha completado
            RefreshToken: "...", // El refresco de sesión se gestiona de forma independiente
            Permissions: permissionsList
        );
    }
}
