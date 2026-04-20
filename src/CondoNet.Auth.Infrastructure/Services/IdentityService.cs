using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace CondoNet.Auth.Infrastructure.Services;

public class IdentityService(IOptions<JwtSettings> jwtSettings) : IIdentityService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    public string HashPassword(string password) => BC.HashPassword(password);

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        var cleanPassword = password.Trim();
        var cleanHash = hashedPassword.Trim();

        return BC.Verify(cleanPassword, cleanHash);
    }

    public string GenerateJwtToken(User user, UserContext context)
    {

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("OrganizationId", context.OrganizationId.ToString()),
            new("RoleId", context.RoleId.ToString()),
            new("Role", context.Role.Name),
            new(ClaimTypes.Role, context.Role.Name)
        };

        // Si el contexto tiene un condominio específico, lo agregamos
        if (context.CondoId.HasValue)
        {
            claims.Add(new("CondoId", context.CondoId.Value.ToString()));
        }

        // Agregamos los permisos del role
        foreach (var p in context.Role.RolePermissions)
        {
            claims.Add(new Claim("permissions", p.Permission.Name));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

}
