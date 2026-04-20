using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.DTOs;
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
                };

            if (context.CondoId.HasValue)
                claims.Add(new("condo_id", context.CondoId.Value.ToString()));

            foreach (var role in context.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            // 2. Generar el Token (usa tu lógica de JwtSecurityTokenHandler)
            var token = "token_generado_con_claims_de_contexto";

            return new LoginResponse(
                Token: token,
                FullName: context.User.FullName,
                Contexts: [], // Opcional o vacío ya que ya seleccionó
                RefreshToken: "...",
                Permissions: [] // Opcional, podrías incluir permisos específicos del contexto
            );
        }
    }
}
