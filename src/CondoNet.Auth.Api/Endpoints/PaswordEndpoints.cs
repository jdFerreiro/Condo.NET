using CondoNet.Auth.Api.DTOs;
using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CondoNet.Auth.Api.Endpoints;

public static class PasswordEndpoints
{
    public static void MapPasswordEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/passwords").WithTags("Seguridad de Contraseñas");

        // 1. Cambio de contraseña (Autenticado)
        group.MapPost("/change", async (
            ChangePasswordRequest request,
            AuthDbContext db,
            IIdentityService identityService,
            ClaimsPrincipal userPrincipal) =>
        {
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId)) return Results.Unauthorized();

            var user = await db.Users.FindAsync(userId);
            if (user == null || !identityService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                return Results.BadRequest("La contraseña actual es incorrecta.");

            user.PasswordHash = identityService.HashPassword(request.NewPassword);

            // Invalidar Refresh Tokens por seguridad tras cambio de clave
            var tokens = await db.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
            tokens.ForEach(t => t.IsRevoked = true);

            await db.SaveChangesAsync();
            return Results.Ok("Contraseña actualizada con éxito.");
        }).RequireAuthorization();

        // 2. Solicitar reseteo (Público)
        group.MapPost("/reset-request", async (ResetPasswordRequest request, AuthDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return Results.Accepted(); // Por seguridad no revelamos si el email existe

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            db.PasswordResetTokens.Add(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            });

            await db.SaveChangesAsync();

            // TODO: Integrar con un servicio de Email para enviar el token
            Console.WriteLine($"[EMAIL MOCK] Token para {user.Email}: {token}");

            return Results.Accepted();
        });

        // 3. Ejecutar reseteo con Token (Público)
        group.MapPost("/reset-execute", async (ExecuteResetRequest request, AuthDbContext db, IIdentityService identityService) =>
        {
            var resetToken = await db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == request.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (resetToken == null) return Results.BadRequest("Token inválido o expirado.");

            resetToken.User.PasswordHash = identityService.HashPassword(request.NewPassword);
            resetToken.IsUsed = true;

            await db.SaveChangesAsync();
            return Results.Ok("Tu contraseña ha sido restablecida.");
        });
    }
}
