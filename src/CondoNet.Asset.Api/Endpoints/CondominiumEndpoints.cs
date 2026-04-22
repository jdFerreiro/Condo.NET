namespace CondoNet.Asset.Api.Endpoints
{
    using CondoNet.Asset.Core.Entities;
    using CondoNet.Asset.Infraestructure.Persistence;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;

    public static class CondominiumEndpoints
    {
        public static void MapCondominiumEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/assets/condominiums")
                .WithTags("Condominiums")
                .RequireAuthorization();

            group.MapGet("", async (AssetDbContext context) =>
                Results.Ok(await context.Condominiums.AsNoTracking().ToListAsync()));

            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Condominiums.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            });

            group.MapPost("", async (CreateCondominiumRequest request, AssetDbContext context) =>
            {
                var entity = new Condominium
                {
                    Name = request.Name,
                    Address = request.Address
                };

                context.Condominiums.Add(entity);
                await context.SaveChangesAsync();
                return Results.Created($"/api/assets/condominiums/{entity.Id}", entity);
            });

            group.MapPut("/{id:guid}", async (Guid id, UpdateCondominiumRequest request, AssetDbContext context) =>
            {
                var entity = await context.Condominiums.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                entity.Name = request.Name;
                entity.Address = request.Address;

                await context.SaveChangesAsync();
                return Results.Ok(entity);
            });

            group.MapDelete("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Condominiums.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                context.Condominiums.Remove(entity);
                await context.SaveChangesAsync();
                return Results.NoContent();
            });
        }

        private sealed record CreateCondominiumRequest(string Name, string Address);
        private sealed record UpdateCondominiumRequest(string Name, string Address);
    }
}
