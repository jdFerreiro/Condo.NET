using CondoNet.Auth.Core.DTOs;
using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.Settings;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace CondoNet.Auth.Infrastructure.Services
{
    public class TokenService(IOptions<JwtSettings> settings) : ITokenService
    {
        private readonly JwtSettings _settings = settings.Value;

        public async Task<LoginResponse> GenerateContextTokenAsync(Guid userId, UserContext context)
        {
            // 1. Crear Claims con la data del Contexto
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("org_id", context.OrganizationId.ToString()),
            new("role_id", context.RoleId.ToString()),
            new(ClaimTypes.Role, context.Role.Name)
        };

            if (context.CondoId.HasValue)
                claims.Add(new("condo_id", context.CondoId.Value.ToString()));

            // 2. Generar el Token (usa tu lógica de JwtSecurityTokenHandler)
            var token = "token_generado_con_claims_de_contexto";

            return new LoginResponse(
                Token: token,
                FullName: context.User.FullName,
                Contexts: new(), // Opcional o vacío ya que ya seleccionó
                RefreshToken: "..."
            );
        }
    }
}
