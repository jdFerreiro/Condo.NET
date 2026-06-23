using CondoNet.Asset.Core.Interfaces;
using CondoNet.Shared.Asset.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CondoNet.Asset.Api.Endpoints;

public static class OrganizationEndpoints
{
    public static void MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assets/organizations")
                       .WithTags("Estructura - Organizaciones");

        // POST: Registrar una nueva empresa administradora global (Acceso SuperAdmin)
        group.MapPost("/", async (RegisterOrganizationRequest request, IOrganizationService service) =>
        {
            var result = await service.RegisterOrganizationAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/assets/organizations/{result.Value}", new { Id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .RequireAuthorization("RequireAdminRole");

        // GET: Obtener el perfil de la organización por ID
        group.MapGet("/{id:guid}", async (Guid id, IOrganizationService service) =>
        {
            var result = await service.GetOrganizationByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        })
        .RequireAuthorization("RequiredAnyRole");

        // PUT: Actualizar datos comerciales de contacto
        group.MapPut("/{id:guid}", async (Guid id, UpdateOrgRequest request, IOrganizationService service) =>
        {
            var result = await service.UpdateOrganizationAsync(id, request);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequireAuthorization("RequireAdminRole");

        // PUT (SaaS): Modificar el plan de suscripción (Free, Premium, etc.)
        group.MapPut("/{id:guid}/plan", async (Guid id, [FromQuery] int plan, IOrganizationService service) =>
        {
            var result = await service.UpdateSubscriptionPlanAsync(id, plan);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequireAuthorization("RequireAdminRole");

        // PATCH: Activar/Suspender de forma lógica las operaciones de la empresa
        group.MapPatch("/{id:guid}/status", async (Guid id, [FromQuery] bool active, IOrganizationService service) =>
        {
            var result = await service.ToggleOrganizationStatusAsync(id, active);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequireAuthorization("RequireAdminRole");
    }
}
