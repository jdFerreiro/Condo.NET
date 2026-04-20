using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared.DTOs;
using CondoNet.Shared.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Infrastructure.Services
{
    public class LogoutService(AuthDbContext db, IPublishEndpoint publishEndpoint) : ILogoutService
    {
        private readonly AuthDbContext _db = db;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
        public async Task<Result<bool>> LogoutAsync(string refreshToken)
        {
            var token = await _db.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (token == null) return Result<bool>.Failure("Token no encontrado");

            token.IsRevoked = true; // Invalidamos la sesión
            await _db.SaveChangesAsync();

            await _publishEndpoint.Publish(new UserLoggedOutEvent(
                token.UserId,
                refreshToken,
                DateTime.UtcNow
            ));

            return Result<bool>.Success(true);
        }

    }
}
