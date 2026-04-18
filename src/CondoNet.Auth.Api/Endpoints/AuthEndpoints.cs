using CondoNet.Auth.Api.DTOs;
using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
                       .WithTags("Autenticación");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            AuthDbContext db,
            IIdentityService identityService) =>
        {
            // Usamos AsNoTracking para una lectura limpia de solo lectura
            var user = await db.Users
                .AsNoTracking()
                .Include(u => u.Contexts)
                .FirstOrDefaultAsync(u => u.Email.Equals(request.Email.ToLower(), StringComparison.CurrentCultureIgnoreCase));

            if (user == null)
                return Results.Json(new { error = "Usuario no encontrado" }, statusCode: 401);

            if (!identityService.VerifyPassword(request.Password, user.PasswordHash))
                return Results.Json(new { error = "Contraseña inválida" }, statusCode: 401);

            var context = user.Contexts.FirstOrDefault();
            if (context == null)
                return Results.Json(new { error = "Usuario sin contexto/rol" }, statusCode: 403);

            var token = identityService.GenerateJwtToken(user, context);
            var refreshToken = identityService.GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Dura mucho más que el JWT
                IsRevoked = false
            };

            db.RefreshTokens.Add(refreshTokenEntity);
            await db.SaveChangesAsync();

            return Results.Ok(new LoginResponse(
                Token: token,
                FullName: user.FullName,
                RoleId: context.Role.Id,
                Role: context.Role.Name,
                OrganizationId: context.OrganizationId,
                CondoId: context.CondoId,
                RefreshToken: refreshToken
            ));
        })
        .AllowAnonymous()
        .WithName("Login");

        // Aquí puedes agregar más: /register, /forgot-password, etc.
    }
}
