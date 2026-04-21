using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared.Auth.DTOs;
using CondoNet.Shared.Auth.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CondoNet.Auth.Infrastructure.Services
{
    public class PasswordService(AuthDbContext db, IIdentityService identityService, IPublishEndpoint publishEndpoint) : IPasswordService
    {
        private readonly AuthDbContext _db = db;
        private readonly IIdentityService _identityService = identityService;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<Result<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || !_identityService.VerifyPassword(currentPassword, user.PasswordHash))
                return Result<bool>.Failure("La contraseña actual es incorrecta.");

            user.PasswordHash = _identityService.HashPassword(newPassword);

            // Seguridad: Revocamos tokens de refresco al cambiar clave
            var tokens = await _db.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
            tokens.ForEach(t => t.IsRevoked = true);

            await _db.SaveChangesAsync();

            await _publishEndpoint.Publish(new PasswordChangedEvent(user.Id, user.Email, DateTime.UtcNow));

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RequestResetAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower());
            if (user == null) return Result<bool>.Success(true); // Retorno silencioso por seguridad

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            });

            await _db.SaveChangesAsync();

            await _publishEndpoint.Publish(new PasswordResetRequestedEvent(user.Id, user.Email, token, DateTime.UtcNow.AddMinutes(15)));

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> ExecuteResetAsync(string token, string newPassword)
        {
            var resetToken = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null) return Result<bool>.Failure("Token inválido o expirado.");

            resetToken.User.PasswordHash = _identityService.HashPassword(newPassword);
            resetToken.IsUsed = true;

            await _db.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}
