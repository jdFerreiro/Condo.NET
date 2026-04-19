using CondoNet.Auth.Api.DTOs;
using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CondoNet.Auth.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/users")
                       .WithTags("Gestión de Usuarios")
                       .RequireAuthorization(); // Solo usuarios con JWT válido

        // 1. Crear nuevo Vecino/Externo
        group.MapPost("/", async (
            CreateUserRequest request,
            AuthDbContext db,
            IIdentityService identityService,
            IPublishEndpoint publishEndpoint,
            ClaimsPrincipal userPrincipal) =>
        {
            // Extraer OrgId del administrador que está creando al usuario
            var orgIdClaim = userPrincipal.FindFirst("org_id")?.Value;
            if (!Guid.TryParse(orgIdClaim, out var orgId)) return Results.Unauthorized();

            // Verificar si el usuario ya existe
            if (await db.Users.AnyAsync(u => u.Email == request.Email))
                return Results.BadRequest("El correo ya está registrado.");

            // Crear Entidad Usuario
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim().ToLower(),
                FullName = request.FullName.Trim(),
                PasswordHash = identityService.HashPassword(request.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Crear el Contexto (Vínculo con el condominio)
            var context = new UserContext
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                OrganizationId = orgId,
                CondoId = request.CondoId,
                RoleId = 2 // Asignar el rol ADMIN por defecto
            };

            db.Users.Add(newUser);
            db.UserContexts.Add(context);
            await db.SaveChangesAsync();

            // 2. EMITIR EVENTO ASÍNCRONO
            await publishEndpoint.Publish(new UserCreatedEvent
            {
                UserId = newUser.Id,
                Email = newUser.Email,
                FullName = newUser.FullName,
                Role = context.Role.Name, // Nombre del rol desde la navegación
                OrganizationId = orgId,
                CondoId = request.CondoId
            });

            return Results.Created($"/api/auth/users/{newUser.Id}",
                new UserResponse(newUser.Id, newUser.Email, newUser.FullName, newUser.IsActive));
        });

        // 2. Listar usuarios del condominio actual
        group.MapGet("/", async (AuthDbContext db, ClaimsPrincipal userPrincipal) =>
        {
            var orgIdClaim = userPrincipal.FindFirst("org_id")?.Value;
            if (!Guid.TryParse(orgIdClaim, out var orgId)) return Results.Unauthorized();

            var users = await db.UserContexts
                .Where(c => c.OrganizationId == orgId)
                .Select(c => new UserResponse(c.User.Id, c.User.Email, c.User.FullName, c.User.IsActive))
                .ToListAsync();

            return Results.Ok(users);
        });
    }
}
