namespace CondoNet.Asset.Api.Endpoints
{
    using CondoNet.Asset.Core.Entities;
    using CondoNet.Asset.Infraestructure.Persistence;
    using CondoNet.Shared.Interfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;

    public static class TowerEndpoints
    {
        public static void MapTowerEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/assets/towers")
                .WithTags("Towers")
                .RequireAuthorization("RequireAdminRole"); // Aplica la directiva de Admin;

            group.MapGet("", async (AssetDbContext context, ITenantService tenantService) =>
            {
                var condominiumId = tenantService.GetCondominiumId();

                Results.Ok(await context.Towers
                                .Where(x => x.CondominiumId == condominiumId)
                                .AsNoTracking()
                                .ToListAsync());
            });

            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Towers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            });

            group.MapPost("", async (CreateTowerRequest request, AssetDbContext context) =>
            {
                var entity = new Tower
                {
                    CondominiumId = request.CondominiumId,
                    Name = request.Name
                };

                context.Towers.Add(entity);
                await context.SaveChangesAsync();
                return Results.Created($"/api/assets/towers/{entity.Id}", entity);
            });

            group.MapPut("/{id:guid}", async (Guid id, UpdateTowerRequest request, AssetDbContext context) =>
            {
                var entity = await context.Towers.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                entity.CondominiumId = request.CondominiumId;
                entity.Name = request.Name;

                await context.SaveChangesAsync();
                return Results.Ok(entity);
            });

            group.MapDelete("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Towers.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                context.Towers.Remove(entity);
                await context.SaveChangesAsync();
                return Results.NoContent();
            });
        }

        private sealed record CreateTowerRequest(Guid CondominiumId, string Name);
        private sealed record UpdateTowerRequest(Guid CondominiumId, string Name);
    }
}
