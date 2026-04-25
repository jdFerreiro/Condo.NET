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
                .WithTags("Condominiums"); // Aplica la directiva de Admin


            group.MapGet("", async (AssetDbContext context, ITenantService tenantService) =>
            {
                var organizationId = tenantService.GetOrganizationId();

                var data = await context.Condominiums
                                .Where(c => c.OrganizationId == organizationId)
                                .AsNoTracking()
                                .ToListAsync();

                return Results.Ok(data);
            })
            .RequireAuthorization("RequiredAnyRole");

            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context, ITenantService tenantService) =>
            {
                var organizationId = tenantService.GetOrganizationId();
                var entity = await context.Condominiums
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Id == id && x.OrganizationId == organizationId);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            })
            .RequireAuthorization("");

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
            })
                .RequireAuthorization("RequireAdminRole");

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
            })
                .RequireAuthorization("RequireAdminRole");

            group.MapDelete("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Condominiums.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                context.Condominiums.Remove(entity);
                await context.SaveChangesAsync();
                return Results.NoContent();
            })
            .RequireAuthorization("RequireAdminRole");

        }

        private sealed record CreateCondominiumRequest(string Name, string Address);
        private sealed record UpdateCondominiumRequest(string Name, string Address);
    }
}
