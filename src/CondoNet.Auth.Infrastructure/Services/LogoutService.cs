using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared;
using CondoNet.Shared.Auth.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Infrastructure.Services;

public class LogoutService(AuthDbContext db, IPublishEndpoint publishEndpoint) : ILogoutService
{
    private readonly AuthDbContext _db = db;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<Result<bool>> LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result<bool>.Failure("El token provisto es inválido.");

        // 1. Buscamos el token en la base de datos de forma estricta.
        // Validamos de antemano que no haya sido revocado previamente para evitar ataques de repetición.
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null)
            return Result<bool>.Failure("Sesión no encontrada en el sistema.");

        if (token.IsRevoked)
            return Result<bool>.Failure("La sesión ya se encontraba cerrada previamente.");

        if (token.ExpiresAt < DateTime.UtcNow)
            return Result<bool>.Failure("La sesión especificada ya ha expirado.");

        // 2. Ejecución atómica de la regla de negocio
        token.IsRevoked = true;
        await _db.SaveChangesAsync();

        // 3. Publicación del evento de integración hacia RabbitMQ mediante MassTransit.
        // Esto le permite a servicios como 'Booking' invalidar memorias cachés o desconectar WebSockets.
        await _publishEndpoint.Publish(new UserLoggedOutEvent(
            token.UserId,
            refreshToken,
            DateTime.UtcNow
        ));

        return Result<bool>.Success(true);
    }
}
