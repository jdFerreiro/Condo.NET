using CondoNet.Asset.Infrastructure.Persistence;
using CondoNet.Shared.Asset.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Asset.Api.Endpoints
{
    public static class AssetEndpoints
    {
        public static void MapAssetEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/assets")
                           .WithTags("Assets Inventory")
                           .RequireAuthorization("RequireAdminRole");

            // 1. Obtener todos los activos de un condominio
            group.MapGet("/condominium/{condominiumId:guid}", async (Guid condominiumId, AssetDbContext context) =>
            {
                var assets = await context.Assets
                                          .Where(a => a.CondominiumId == condominiumId)
                                          .ToListAsync();
                return Results.Ok(assets);
            })
            .WithName("GetAssetsByCondo");

            // 2. Crear un nuevo Activo (Solo Admins)
            group.MapPost("/", async (Core.Entities.Asset asset, AssetDbContext context, IPublishEndpoint publishEndpoint) =>
            {
                context.Assets.Add(asset);
                await context.SaveChangesAsync();

                // Emitir evento de integración a RabbitMQ
                await publishEndpoint.Publish(new AssetCreated(
                    asset.OrganizationId,
                    asset.CondominiumId,
                    asset.Id,
                    asset.Name,
                    (int)asset.Category
                ));

                return Results.Created($"/api/assets/{asset.Id}", asset);
            })
            .WithName("CreateAsset");

            // 3. Vincular un estacionamiento/maletero a una Unidad (Apto)
            group.MapPatch("/{id:guid}/link-to-unit", async (
                Guid id,
                Guid unitId,
                AssetDbContext context,
                IPublishEndpoint publishEndpoint) =>
            {
                var asset = await context.Assets.FindAsync(id);
                if (asset is null) return Results.NotFound();

                asset.LinkedUnitId = unitId;
                await context.SaveChangesAsync();

                // Emitir evento para que otros servicios sepan que ya no es común
                await publishEndpoint.Publish(new AssetLinkedToUnit(
                    asset.OrganizationId,
                    asset.Id,
                    unitId
                ));

                return Results.Ok(new { message = "Activo vinculado exitosamente" });
            })
            .WithName("LinkAssetToUnit");
        }
    }
}
