namespace CondoNet.Asset.Api.Endpoints
{
    using CondoNet.Asset.Core.Interfaces;
    using CondoNet.Asset.Infraestructure.Persistence;
    using CondoNet.Shared.Asset.DTOs;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;

    public static class AssetEndpoints
    {
        public static void MapAssetEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/assets")
                           .WithTags("Assets")
                           .RequireAuthorization(); // Requiere JWT con org_id

            // Endpoint para Carga Masiva de Unidades
            group.MapPost("/units/bulk-import", async (BulkImportRequest request, IAssetService assetService) =>
            {
                try
                {
                    var result = await assetService.BulkImportUnitsAsync(request);
                    return Results.Ok(new { message = "Unidades cargadas exitosamente" });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (Exception)
                {
                    return Results.Problem("Ocurrió un error interno al procesar la carga.");
                }
            })
            .WithName("BulkImportUnits");
            // .WithOpenApi(operation => new(operation) { Summary = "Carga masiva de unidades con validación de alícuotas" });

            // Endpoint para obtener resumen del condominio (Basado en el filtro global del DbContext)
            group.MapGet("/condominiums/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var condo = await context.Condominiums
                    .Include(c => c.Units)
                    .FirstOrDefaultAsync(c => c.Id == id);

                return condo is not null ? Results.Ok(condo) : Results.NotFound();
            });
        }
    }
}
