using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared;
using CondoNet.Shared.Auth.DTOs;
using CondoNet.Shared.Auth.Events; // Asegura la importación de tus contratos de mensajería
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Infrastructure.Services;

public class AuthService(AuthDbContext db, IIdentityService identityService, IPublishEndpoint publishEndpoint) : IAuthService
{
    private readonly AuthDbContext _db = db;
    private readonly IIdentityService _identityService = identityService;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // 1. Normalizar el string de entrada de forma preventiva
        var normalizedEmail = request.Email.Trim().ToLower();

        // 2. Buscar usuario con su árbol completo de accesos. 
        // Eliminamos el .ToLower() de la columna para preservar el uso de índices indexados en SQL Server.
        var user = await _db.Users
            .Include(u => u.Contexts)
                .ThenInclude(c => c.Roles)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user == null || !_identityService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure("Credenciales inválidas");
        }

        // 3. Filtrar los contextos que se encuentran plenamente operativos
        var activeContexts = user.Contexts
            .Where(c => c.Status == ContextStatus.Active)
            .ToList();

        if (activeContexts.Count == 0)
        {
            return Result<LoginResponse>.Failure("No tienes perfiles activos asociados a ninguna organización.");
        }

        // 4. Generación de credenciales criptográficas y tokens de sesión
        // Seleccionamos el primer entorno activo por defecto para firmar el JWT inicial
        var defaultContext = activeContexts.First();
        var token = _identityService.GenerateJwtToken(user, defaultContext);
        var refreshToken = _identityService.GenerateRefreshToken();

        // 5. Persistir el Token de Refresco para control de sesiones activas
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

        // 6. Aplanar los permisos a nivel de aplicación para autorizaciones rápidas
        var userPermissions = activeContexts
            .SelectMany(c => c.Roles.SelectMany(p => p.RolePermissions))
            .Select(rp => rp.Permission) // Proyectamos la entidad Permission directamente
            .DistinctBy(p => p.Id)       // Evitamos duplicados en memoria usando el Id de forma eficiente
            .Select(p => new Permissions(
                Id: p.Id,
                Name: p.Name,
                Description: p.Description,
                DisplayOrder: p.DisplayOrder,
                Path: p.Path,
                Icon: p.Icon // Mapeo directo al nuevo campo optimizado de iconos de la UI
            ))
            .ToList();

        // 7. Mapear los entornos multi-tenant disponibles para que el cliente pueda conmutar
        var contextResponses = activeContexts.Select(c => new AvailableContextResponse(
            c.Id,
            c.OrganizationId,
            c.CondoId,
            [.. c.Roles.Select(r => r.Name)]
        )).ToList();

        // 8. Publicación del evento de auditoría a través de MassTransit hacia RabbitMQ.
        // Incluye la Organización y el Condominio del perfil activo inicial para el Shipper de Logs.
        await _publishEndpoint.Publish(new UserLoggedInEvents(
            user.Id,
            defaultContext.OrganizationId,
            defaultContext.CondoId,
            user.FullName,
            DateTime.UtcNow
        ));

        return Result<LoginResponse>.Success(new LoginResponse(
            Token: token,
            FullName: user.FullName,
            Contexts: contextResponses,
            RefreshToken: refreshToken,
            Permissions: userPermissions
        ));
    }
}
