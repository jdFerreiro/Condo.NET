using CondoNet.Asset.Core.Interfaces;
using CondoNet.Shared.Asset.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CondoNet.Asset.Api.Endpoints;

public static class TowerAndUnitEndpoints
{
    public static void MapTowerAndUnitEndpoints(this IEndpointRouteBuilder app)
    {
        // --- GRUPO TORRES / BLOQUES ---
        var towerGroup = app.MapGroup("/api/assets/towers").WithTags("Infraestructura - Torres");

        towerGroup.MapPost("/", async ([FromQuery] string name, ITowerService service) =>
        {
            var result = await service.CreateTowerAsync(name);
            return result.IsSuccess ? Results.Created($"/api/assets/towers/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        towerGroup.MapGet("/", async (ITowerService service) =>
        {
            var result = await service.GetTowersByCondoAsync();
            return Results.Ok(result);
        }).RequireAuthorization("RequiredAnyRole");

        towerGroup.MapPut("/{id:guid}", async (Guid id, [FromQuery] string newName, ITowerService service) =>
        {
            var result = await service.UpdateTowerAsync(id, newName);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        towerGroup.MapDelete("/{id:guid}", async (Guid id, ITowerService service) =>
        {
            var result = await service.DeleteTowerAsync(id);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        // --- GRUPO UNIDADES / DEPARTAMENTOS ---
        var unitGroup = app.MapGroup("/api/assets/units").WithTags("Infraestructura - Unidades");

        // POST: Carga e Importación Masiva (Excel/CSV parseado desde el Frontend)
        unitGroup.MapPost("/bulk-import", async (BulkImportRequest request, IAssetService service) =>
        {
            var success = await service.BulkImportUnitsAsync(request);
            return success ? Results.Ok(new { message = "Lote de unidades e infraestructura importado con éxito." }) : Results.BadRequest("Fallo en la importación masiva.");
        }).RequireAuthorization("RequireAssetRole");

        unitGroup.MapPost("/", async (CreateUnitRequest request, IUnitService service) =>
        {
            var result = await service.CreateUnitAsync(request);
            return result.IsSuccess ? Results.Created($"/api/assets/units/{result.Value}", new { Id = result.Value }) : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        unitGroup.MapGet("/tower/{towerId:guid}", async (Guid towerId, IUnitService service) =>
        {
            var result = await service.GetUnitsByTowerAsync(towerId);
            return Results.Ok(result.Value);
        }).RequireAuthorization("RequiredAnyRole");

        unitGroup.MapGet("/owner/{ownerEmail}", async (string ownerEmail, IUnitService service) =>
        {
            var result = await service.GetUnitsByOwnerAsync(ownerEmail);
            return Results.Ok(result);
        }).RequireAuthorization("RequiredAnyRole");

        unitGroup.MapPut("/{id:guid}", async (Guid id, UpdateUnitRequest request, IUnitService service) =>
        {
            var result = await service.UpdateUnitAsync(id, request);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");

        unitGroup.MapPost("/change-ownership", async ([FromQuery] Guid userId, [FromQuery] Guid oldUnitId, [FromQuery] Guid newUnitId, IUnitService service) =>
        {
            await service.ChangeUnitAsync(userId, oldUnitId, newUnitId);
            return Results.Ok(new { message = "Mudanza o traspaso físico de propiedad procesado." });
        }).RequireAuthorization("RequireAssetRole");

        unitGroup.MapDelete("/{id:guid}", async (Guid id, IUnitService service) =>
        {
            var result = await service.DeleteUnitAsync(id);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization("RequireAssetRole");
    }
}
