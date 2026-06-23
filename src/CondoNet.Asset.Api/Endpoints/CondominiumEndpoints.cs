using CondoNet.Asset.Core.Interfaces;
using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Asset.Api.Endpoints;

public static class CondominiumEndpoints
{
    public static void MapCondominiumEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assets/condominiums")
                       .WithTags("Estructura - Condominios");

        // POST: Dar de alta un condominio en la Organización del administrador logueado
        group.MapPost("/", async (CreateCondominiumRequest request, ICondominiumService service) =>
        {
            var result = await service.CreateCondominiumAsync(request);
            return result.IsSuccess
                ? Results.Created($"/api/assets/condominiums/{result.Value}", new { Id = result.Value })
                : Results.BadRequest(result.Error);
        })
        .RequireAuthorization("RequireAdminRole");

        // GET: Detalle de un condominio específico (Valida aislamiento interno en el servicio)
        group.MapGet("/{id:guid}", async (Guid id, ICondominiumService service) =>
        {
            var result = await service.GetCondominiumByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        })
        .RequireAuthorization("RequiredAnyRole");

        // GET: Listar todos los condominios que administra la organización activa del token
        group.MapGet("/", async (ICondominiumService service) =>
        {
            var result = await service.GetCondominiumsByOrganizationAsync();
            return Results.Ok(result.Value);
        })
        .RequireAuthorization("RequireAdminRole");

        // PUT: Actualizar datos de infraestructura o porcentaje de fondo de reserva
        group.MapPut("/{id:guid}", async (Guid id, UpdateCondominiumRequest request, ICondominiumService service) =>
        {
            var result = await service.UpdateCondominiumAsync(id, request);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequireAuthorization("RequireAdminRole");

        // DELETE: Remover un condominio (Valida integridad de unidades en el servicio)
        group.MapDelete("/{id:guid}", async (Guid id, ICondominiumService service) =>
        {
            var result = await service.DeleteCondominiumAsync(id);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        })
        .RequireAuthorization("RequireAdminRole");
    }
}
