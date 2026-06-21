using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared.Auth.DTOs;
using CondoNet.Shared.Auth.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CondoNet.Auth.Infrastructure.Services;

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

        // Seguridad estricta: Invalidamos todas las sesiones abiertas en otros dispositivos
        var tokens = await _db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
        tokens.ForEach(t => t.IsRevoked = true);

        await _db.SaveChangesAsync();

        await _publishEndpoint.Publish(new PasswordChangedEvent(user.Id, user.Email, DateTime.UtcNow));

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RequestResetAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return Result<bool>.Success(true);

        // Limpieza de variable fuera de la lambda para garantizar el uso de índices de SQL Server
        var normalizedEmail = email.Trim().ToLower();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (user == null) return Result<bool>.Success(true); // Retorno silencioso (Previene enumeración)

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

    /// <summary>
    /// REGLA DE NEGOCIO: Ejecuta el reseteo de la clave e invalida de forma atómica todas las sesiones previas.
    /// </summary>
    public async Task<Result<bool>> ExecuteResetAsync(string token, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result<bool>.Failure("El token de recuperación provisto es inválido.");

        // 1. Buscamos el token de recuperación y cargamos la navegación del usuario
        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

        if (resetToken == null)
            return Result<bool>.Failure("El enlace de recuperación es inválido, ya fue utilizado o ha expirado.");

        // 2. Modificación criptográfica de credenciales
        resetToken.User.PasswordHash = _identityService.HashPassword(newPassword);
        resetToken.IsUsed = true;

        // 3. REGLA DE SEGURIDAD ADICIONAL: Expulsamos al usuario de todos sus dispositivos activos
        var activeSessions = await _db.RefreshTokens
            .Where(t => t.UserId == resetToken.UserId && !t.IsRevoked)
            .ToListAsync();

        activeSessions.ForEach(t => t.IsRevoked = true);

        await _db.SaveChangesAsync();

        // 4. Notificación asíncrona de cambio completado con éxito
        await _publishEndpoint.Publish(new PasswordChangedEvent(resetToken.User.Id, resetToken.User.Email, DateTime.UtcNow));

        return Result<bool>.Success(true);
    }
}
