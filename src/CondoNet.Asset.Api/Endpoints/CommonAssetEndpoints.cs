namespace CondoNet.Asset.Api.Endpoints
{
    using CondoNet.Asset.Core.Entities;
    using CondoNet.Asset.Infraestructure.Persistence;
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
                .RequireAuthorization();

            group.MapGet("", async (AssetDbContext context) =>
                Results.Ok(await context.Assets.AsNoTracking().ToListAsync()));

            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            });

            group.MapPost("", async (CreateAssetRequest request, AssetDbContext context) =>
            {
                var entity = new Asset
                {
                    CondominiumId = request.CondominiumId,
                    Name = request.Name,
                    Category = request.Category,
                    IsRentable = request.IsRentable,
                    DefaultRentalPrice = request.DefaultRentalPrice,
                    LinkedUnitId = request.LinkedUnitId,
                    Status = request.Status
                };

                context.Assets.Add(entity);
                await context.SaveChangesAsync();
                return Results.Created($"/api/assets/common-assets/{entity.Id}", entity);
            });

            group.MapPut("/{id:guid}", async (Guid id, UpdateAssetRequest request, AssetDbContext context) =>
            {
                var entity = await context.Assets.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                entity.CondominiumId = request.CondominiumId;
                entity.Name = request.Name;
                entity.Category = request.Category;
                entity.IsRentable = request.IsRentable;
                entity.DefaultRentalPrice = request.DefaultRentalPrice;
                entity.LinkedUnitId = request.LinkedUnitId;
                entity.Status = request.Status;

                await context.SaveChangesAsync();
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
