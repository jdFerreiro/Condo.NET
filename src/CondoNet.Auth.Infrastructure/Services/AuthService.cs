using CondoNet.Auth.Core.DTOs;
using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Infrastructure.Services
{
    public class AuthService(AuthDbContext db, IIdentityService identityService) : IAuthService
    {
        private readonly AuthDbContext _db = db;
        private readonly IIdentityService _identityService = identityService;

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLower();

            // 1. Buscamos usuario con sus contextos y roles
            var user = await _db.Users
                .Include(u => u.Contexts)
                    .ThenInclude(c => c.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null || !_identityService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Result<LoginResponse>.Failure("Credenciales inválidas");
            }

            // 2. Filtramos contextos activos
            var activeContexts = user.Contexts
                .Where(c => c.Status == ContextStatus.Active)
                .ToList();

            if (activeContexts.Count == 0)
            {
                return Result<LoginResponse>.Failure("No tienes perfiles activos.");
            }

            // 3. Generamos Tokens (JWT inicial y RefreshToken)
            // Usamos el primer contexto por defecto para el token inicial o uno genérico
            var defaultContext = activeContexts.First();
            var token = _identityService.GenerateJwtToken(user, defaultContext);
            var refreshToken = _identityService.GenerateRefreshToken();

            // 4. Persistir Refresh Token
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _db.RefreshTokens.Add(refreshTokenEntity);
            await _db.SaveChangesAsync();

            // 5. Mapear respuesta
            var contextResponses = activeContexts.Select(c => new AvailableContextResponse(
                c.Id,
                c.OrganizationId,
                c.CondoId,
                c.RoleId,
                c.Role // Aquí pasamos el objeto Role completo o el nombre según tu record
            )).ToList();

            return Result<LoginResponse>.Success(new LoginResponse(
                Token: token,
                FullName: user.FullName,
                Contexts: contextResponses,
                RefreshToken: refreshToken
            ));
        }
    }
}
