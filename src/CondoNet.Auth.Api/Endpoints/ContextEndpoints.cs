using CondoNet.Auth.Core.Interfaces;
using CondoNet.Shared.DTOs;
using System.Security.Claims;

namespace CondoNet.Auth.Api.Endpoints
{
    public static class ContextEndpoints
    {
        public static void MapContextEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth/context")
                .WithTags("Gestión de Contextos")
                .RequireAuthorization();

            // 1. Listar contextos disponibles
            group.MapGet("/available", async (IContextService contextService, ClaimsPrincipal user) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var contexts = await contextService.GetUserContextsAsync(userId);
                return Results.Ok(contexts);
            })
            .WithName("Available");

            // 2. Seleccionar un contexto y obtener el token final
            group.MapPost("/select", async (SelectContextRequest request, IContextService contextService, ITokenService tokenService, ClaimsPrincipal user) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // Validamos el contexto solicitado
                var context = await contextService.ValidateAndGetContextAsync(userId, request.ContextId);

                if (context is null)
                    return Results.Problem("Contexto no válido o inactivo", statusCode: 404);

                // Generamos el token que ya incluye OrganizationId, CondoId y Role
                var loginResponse = await tokenService.GenerateContextTokenAsync(userId, context);

                return Results.Ok(loginResponse);
            })
            .WithName("SwitchContext");

        }
    }
}
