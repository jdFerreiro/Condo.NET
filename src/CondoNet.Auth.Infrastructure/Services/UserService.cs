using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared.Auth.DTOs;
using CondoNet.Shared.Auth.Events; // Asegúrate de tener definidos aquí ambos eventos
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Infrastructure.Services;

public class UserService(AuthDbContext db, IIdentityService identityService, IPublishEndpoint publishEndpoint) : IUserService
{
    private readonly AuthDbContext _db = db;
    private readonly IIdentityService _identityService = identityService;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        // 1. Validaciones de negocio tradicionales
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Result<UserResponse>.Failure("El correo ya está registrado.");

        var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Resident");
        if (defaultRole == null)
            return Result<UserResponse>.Failure("Configuración del sistema errónea: Rol base no encontrado.");

        // 2. Creación de la cuenta base de usuario
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLower(),
            FullName = request.FullName.Trim(),
            PasswordHash = _identityService.HashPassword(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Creación del primer contexto específico del usuario
        var initialContext = new UserContext
        {
            Id = Guid.NewGuid(),
            UserId = newUser.Id,
            OrganizationId = request.OrganizationId,
            CondoId = request.CondoId,
            Roles = [defaultRole]
        };

        // 4. Estrategia transaccional atómica de EF Core
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.Users.Add(newUser);
                _db.UserContexts.Add(initialContext);
                await _db.SaveChangesAsync();

                // EVENTO 1: Nace el usuario en el ecosistema (Conserva sus propiedades originales fijas)
                await _publishEndpoint.Publish(new UserCreatedEvent(
                    newUser.Id,
                    newUser.Email,
                    newUser.FullName
                ));

                var rolesAsignados = initialContext.Roles.Select(r => r.Name).ToList();

                // EVENTO 2: El usuario es asignado a su primer entorno (Maneja la lógica del Tenant de forma asíncrona)
                await _publishEndpoint.Publish(new UserContextAssignedEvent(
                    initialContext.Id,
                    newUser.Id,
                    initialContext.OrganizationId,
                    initialContext.CondoId,
                    rolesAsignados
                ));

                await transaction.CommitAsync();

                return Result<UserResponse>.Success(
                    new UserResponse(newUser.Id, newUser.Email, newUser.FullName, newUser.IsActive));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Result<UserResponse>.Failure("Error al crear el usuario.");
            }
        });
    }

    public async Task<Result<List<UserResponse>>> GetUsersByOrganizationAsync(Guid orgId)
    {
        var users = await _db.UserContexts
            .Where(c => c.OrganizationId == orgId)
            .Select(c => new UserResponse(c.User.Id, c.User.Email, c.User.FullName, c.User.IsActive))
            .Distinct()
            .ToListAsync();

        return Result<List<UserResponse>>.Success(users);
    }
}
