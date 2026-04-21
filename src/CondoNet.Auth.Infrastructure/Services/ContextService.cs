using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared.Auth.DTOs;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Infrastructure.Services
{

    public class ContextService(AuthDbContext context, ITokenService tokenService, IPublishEndpoint publishEndpoint) : IContextService
    {
        private readonly AuthDbContext _context = context;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<List<AvailableContextResponse>> GetUserContextsAsync(Guid userId)
        {
            // Consultamos la tabla UserContexts filtrando por el usuario
            // e incluimos el Rol para tener el nombre descriptivo
            return await _context.UserContexts
                .AsNoTracking()
                .Include(c => c.Roles)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission) // Si necesitas permisos en el futuro
                .Where(c => c.UserId == userId && c.Status == ContextStatus.Active)
                .Select(c => new AvailableContextResponse(
                    c.Id,                             // ContextId
                    c.OrganizationId,                 // OrganizationId
                    c.CondoId,                        // CondoId
                    c.Roles.Select(r => r.Name).ToList() // Roles (vienen    de la navegación)
                ))
                .ToListAsync();
        }

        public async Task<UserContext?> ValidateAndGetContextAsync(Guid userId, Guid contextId)
        {
            // Este método es crucial para el endpoint /select-context
            // Verifica que el contexto realmente le pertenezca a quien lo pide
            return await _context.UserContexts
                .Include(c => c.Roles)
                .Include(c => c.User) // Necesario para sacar el FullName en el token final
                .FirstOrDefaultAsync(c => c.Id == contextId &&
                                         c.UserId == userId &&
                                         c.Status == ContextStatus.Active);
        }

    }
}
