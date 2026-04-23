namespace CondoNet.Asset.Api.Endpoints
{
    using CondoNet.Asset.Core.Entities;
    using CondoNet.Asset.Core.Interfaces;
    using CondoNet.Asset.Infraestructure.Persistence;
    using CondoNet.Shared.Asset.Events;
    using MassTransit;
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
                .RequireAuthorization("RequireAdminRole"); // Aplica la directiva de Admin


            group.MapGet("", async (AssetDbContext context) =>
                Results.Ok(await context.Condominiums.AsNoTracking().ToListAsync()));

            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Condominiums.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            });

            group.MapPost("", async (CreateCondominiumRequest request, AssetDbContext context, IPublishEndpoint publishEndpoint, ITenantService tenantService) =>
            {
                var entity = new Condominium
                {
                    Name = request.Name,
                    Address = request.Address
                };

                context.Condominiums.Add(entity);
                await context.SaveChangesAsync();

                var organizationId = tenantService.GetOrganizationId();
                var organization = await context.Organizations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == organizationId);

                await publishEndpoint.Publish(new CondominiumCreated(
                    organizationId,
                    entity.Id,
                    entity.Name,
                    organization?.TaxId ?? string.Empty,
                    organization?.BaseCurrency ?? "USD",
                    0m
                ));

                return Results.Created($"/api/assets/condominiums/{entity.Id}", entity);
            });

            group.MapPut("/{id:guid}", async (Guid id, UpdateCondominiumRequest request, AssetDbContext context, IPublishEndpoint publishEndpoint, ITenantService tenantService) =>
            {
                var entity = await context.Condominiums.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                entity.Name = request.Name;
                entity.Address = request.Address;

                await context.SaveChangesAsync();

                await publishEndpoint.Publish(new CondominiumUpdated(
                    tenantService.GetOrganizationId(),
                    entity.Id,
                    entity.Name,
                    0m
                ));

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
