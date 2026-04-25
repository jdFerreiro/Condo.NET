namespace CondoNet.Asset.Api.Endpoints
{
    using CondoNet.Asset.Core.Entities;
    using CondoNet.Asset.Infraestructure.Persistence;
    using CondoNet.Shared.Asset.Events;
    using CondoNet.Shared.Interfaces;
    using MassTransit;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;

    public static class CommonAssetEndpoints
    {
        public static void MapCommonAssetEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/assets/common-assets")
                .WithTags("CommonAssets")
                .RequireAuthorization("RequireAdminRole");

            group.MapGet("", async (AssetDbContext context, ITenantService tenantService) =>
            {

                var condominiumId = tenantService.GetCondominiumId();

                return Results.Ok(await context.Assets
                                            .Where(x => x.CondominiumId == condominiumId)
                                            .AsNoTracking()
                                            .ToListAsync());
            });

            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context, ITenantService tenantService) =>
            {
                var condominiumId = tenantService.GetCondominiumId();

                var entity = await context.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CondominiumId == condominiumId);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            });

            group.MapPost("", async (CreateAssetRequest request, AssetDbContext context, IPublishEndpoint publishEndpoint, ITenantService tenantService) =>
            {
                var organizationId = tenantService.GetOrganizationId();

                var entity = new Asset
                {
                    CondominiumId = request.CondominiumId,
                    Name = request.Name,
                    Category = request.Category,
                    IsRentable = request.IsRentable,
                    DefaultRentalPrice = request.DefaultRentalPrice,
                    LinkedUnitId = request.LinkedUnitId,
                    Status = request.Status,
                    OrganizationId = organizationId
                };

                context.Assets.Add(entity);
                await context.SaveChangesAsync();

                await publishEndpoint.Publish(new AssetCreated(
                    organizationId,
                    entity.CondominiumId,
                    entity.Id,
                    entity.Name,
                    (int)entity.Category
                ));

                if (entity.LinkedUnitId.HasValue)
                {
                    await publishEndpoint.Publish(new AssetLinkedToUnit(
                        organizationId,
                        entity.Id,
                        entity.LinkedUnitId.Value
                    ));
                }

                return Results.Created($"/api/assets/common-assets/{entity.Id}", entity);
            });

            group.MapPut("/{id:guid}", async (Guid id, UpdateAssetRequest request, AssetDbContext context, IPublishEndpoint publishEndpoint, ITenantService tenantService) =>
            {
                var organizationId = tenantService.GetOrganizationId();

                var entity = await context.Assets.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                var oldStatus = entity.Status;
                var oldLinkedUnitId = entity.LinkedUnitId;
                var oldOrganizationId = entity.OrganizationId;

                entity.CondominiumId = request.CondominiumId;
                entity.Name = request.Name;
                entity.Category = request.Category;
                entity.IsRentable = request.IsRentable;
                entity.DefaultRentalPrice = request.DefaultRentalPrice;
                entity.LinkedUnitId = request.LinkedUnitId;
                entity.Status = request.Status;
                entity.OrganizationId = oldOrganizationId; // Ensure organization ID is not changed

                await context.SaveChangesAsync();

                if (oldStatus != entity.Status)
                {
                    await publishEndpoint.Publish(new AssetStatusChanged(
                        organizationId,
                        entity.CondominiumId,
                        entity.Id,
                        (int)oldStatus,
                        (int)entity.Status
                    ));
                }

                if (oldLinkedUnitId != entity.LinkedUnitId && entity.LinkedUnitId.HasValue)
                {
                    await publishEndpoint.Publish(new AssetLinkedToUnit(
                        organizationId,
                        entity.Id,
                        entity.LinkedUnitId.Value
                    ));
                }

                return Results.Ok(entity);
            });

            group.MapDelete("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Assets.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                context.Assets.Remove(entity);
                await context.SaveChangesAsync();
                return Results.NoContent();
            });
        }

        private sealed record CreateAssetRequest(Guid CondominiumId, string Name, AssetType Category, bool IsRentable, decimal? DefaultRentalPrice, Guid? LinkedUnitId, AssetStatus Status);
        private sealed record UpdateAssetRequest(Guid CondominiumId, string Name, AssetType Category, bool IsRentable, decimal? DefaultRentalPrice, Guid? LinkedUnitId, AssetStatus Status);
    }
}
