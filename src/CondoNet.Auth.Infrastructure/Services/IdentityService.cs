using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace CondoNet.Auth.Infrastructure.Services;

public class IdentityService(IConfiguration config) : IIdentityService
{
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
            new("fullMame", user.FullName),
            new("orgId", context.OrganizationId.ToString()),
            new("roleId", context.Role.Id.ToString()),
            new("roleName", context.Role.Name),
            new("condoId", context.CondoId?.ToString() ?? string.Empty)
        };

        foreach (var p in context.Role.RolePermissions)
        {
            claims.Add(new Claim("permissions", p.Permission.Name));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["JwtSettings:Issuer"],
            audience: config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
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
