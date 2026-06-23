using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared;
using CondoNet.Shared.Auth.DTOs;
using CondoNet.Shared.Auth.Events; // Asegúrate de importar tus contratos compartidos
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Infrastructure.Services;

public class ContextService(AuthDbContext context, ITokenService tokenService, IPublishEndpoint publishEndpoint) : IContextService
{
    private readonly AuthDbContext _context = context;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<List<AvailableContextResponse>> GetUserContextsAsync(Guid userId)
    {
        return await _context.UserContexts
            .AsNoTracking()
            .Include(c => c.Roles)
            .Where(c => c.UserId == userId && c.Status == ContextStatus.Active)
            .Select(c => new AvailableContextResponse(
                c.Id,
                c.OrganizationId,
                c.CondoId,
                c.Roles.Select(r => r.Name).ToList()
            ))
            .ToListAsync();
    }

    public async Task<UserContext?> ValidateAndGetContextAsync(Guid userId, Guid contextId)
    {
        return await _context.UserContexts
            .Include(c => c.Roles)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == contextId &&
                                     c.UserId == userId &&
                                     c.Status == ContextStatus.Active);
    }

    /// <summary>
    /// REGLA DE NEGOCIO: Asigna a un usuario existente un nuevo entorno de Organización o Condominio.
    /// </summary>
    public async Task<Result<Guid>> AssignContextAsync(AssignContextRequest request)
    {
        // 1. Validar la existencia previa del usuario objetivo
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
            return Result<Guid>.Failure("El usuario especificado no existe en el sistema.");

        // 2. Control de Multi-Tenant: Evitar duplicación de contextos activos para el mismo condominio
        var contextExists = await _context.UserContexts.AnyAsync(c =>
            c.UserId == request.UserId &&
            c.CondoId == request.CondoId &&
            c.Status == ContextStatus.Active);

        if (contextExists)
            return Result<Guid>.Failure("El usuario ya cuenta con un contexto activo para este condominio.");

        // 3. Validar y cargar las entidades de los Roles solicitados
        var roles = await _context.Roles
            .Where(r => request.RoleNames.Contains(r.Name))
            .ToListAsync();

        if (roles.Count != request.RoleNames.Count)
            return Result<Guid>.Failure("Uno o más roles solicitados no son válidos en el sistema.");

        // 4. Construcción de la nueva entidad de asignación
        var newContext = new UserContext
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            OrganizationId = request.OrganizationId,
            CondoId = request.CondoId,
            Status = ContextStatus.Active,
            Roles = roles
        };

        // 5. Patrón transaccional para asegurar la Consistencia Eventual
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.UserContexts.Add(newContext);
                await _context.SaveChangesAsync();

                // 6. Publicación del evento hacia RabbitMQ utilizando la colección plana de strings
                var rolesFlatList = roles.Select(r => r.Name).ToList();

                await _publishEndpoint.Publish(new UserContextAssignedEvent(
                    newContext.Id,
                    newContext.UserId,
                    newContext.OrganizationId,
                    newContext.CondoId,
                    rolesFlatList
                ));

                await transaction.CommitAsync();
                return Result<Guid>.Success(newContext.Id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
