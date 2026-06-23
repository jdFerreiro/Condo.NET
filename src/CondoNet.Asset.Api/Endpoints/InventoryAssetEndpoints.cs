using CondoNet.Asset.Core.Interfaces;
using CondoNet.Shared.Asset.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CondoNet.Asset.Api.Endpoints;

public static class InventoryAssetEndpoints
{
    public static void MapInventoryAssetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/assets/inventory").WithTags("Inventario - Bienes y Maquinarias");

        // POST: Registrar un bien común o privado (Estacionamiento, Maletero, Cancha, Piscina)
        group.MapPost("/", async (RegisterAssetRequest request, IInventoryAssetService service) =>
        {
            var result = await service.RegisterAssetAsync(request);
            return result.IsSuccess ? Results.Created($"/api/assets/inventory/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        // GET: Listar todo el inventario físico del condominio activo
        group.MapGet("/", async (IInventoryAssetService service) =>
        {
            var result = await service.GetActiveAssetsAsync();
            return Results.Ok(result);
        }).RequireAuthorization("RequiredAnyRole");

        group.MapPut("/{id:guid}", async (Guid id, UpdateAssetRequest request, IInventoryAssetService service) =>
        {
            var result = await service.UpdateAssetAsync(id, request);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        // PATCH: Vincular o desvincular una cochera/maletero a un departamento específico
        group.MapPatch("/{id:guid}/link-unit", async (Guid id, [FromQuery] Guid? unitId, IInventoryAssetService service) =>
        {
            var result = await service.LinkAssetToUnitAsync(id, unitId);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        // PATCH: Modificar el estado operacional del activo (ej: Poner en 'Mantenimiento' para congelar reservas de Booking)
        group.MapPatch("/{id:guid}/operational-status", async (Guid id, [FromQuery] int status, IInventoryAssetService service) =>
        {
            var result = await service.UpdateAssetStatusAsync(id, status);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        group.MapDelete("/{id:guid}", async (Guid id, IInventoryAssetService service) =>
        {
            var result = await service.DeleteAssetAsync(id);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        // POST: Registrar Maquinaria e Infraestructura Técnica (Ascensores, Plantas Eléctricas, etc.)
        group.MapPost("/critical-equipment", async (RegisterEquipmentRequest request, IInventoryAssetService service) =>
        {
            var result = await service.RegisterCriticalEquipmentAsync(request);
            return result.IsSuccess ? Results.Created($"/api/assets/inventory/critical-equipment/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        // GET: Listar la maquinaria del edificio para paneles de mantenimiento
        group.MapGet("/critical-equipment", async (IInventoryAssetService service) =>
        {
            var result = await service.GetCriticalEquipmentsAsync();
            return Results.Ok(result);
        }).RequireAuthorization("RequiredAnyRole");
    }
}
