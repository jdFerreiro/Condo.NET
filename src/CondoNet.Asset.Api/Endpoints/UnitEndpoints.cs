namespace CondoNet.Asset.Api.Endpoints
{
    using CondoNet.Asset.Core.Entities;
    using CondoNet.Asset.Core.Interfaces;
    using CondoNet.Asset.Infraestructure.Persistence;
    using CondoNet.Shared.Asset;
    using CondoNet.Shared.Asset.DTOs;
    using CondoNet.Shared.Asset.Events;
    using MassTransit;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;

    public static class UnitEndpoints
    {
        public static void MapUnitEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/assets/units")
                .WithTags("Units")
                .RequireAuthorization("RequireAdminRole"); // Aplica la directiva de Admin

            group.MapPost("/bulk-import", async (BulkImportRequest request, IPublishEndpoint publishEndpoint, ITenantService tenantService) =>
            {
                var importId = Guid.NewGuid();
                var orgId = tenantService.GetOrganizationId();

                // Encolamos el comando en RabbitMQ
                await publishEndpoint.Publish(new ProcessBulkImportCommand(
                    importId,
                    request.CondominiumId,
                    orgId,
                    request.Units
                ));

                // Respondemos de inmediato con 202 Accepted
                return Results.Accepted($"/api/assets/imports/{importId}", new
                {
                    message = "Carga recibida y en proceso.",
                    importId
                });
            })
            .WithName("BulkImportUnits");

            group.MapGet("", async (AssetDbContext context, ITenantService tenantService) =>
            {
                var condominiumId = tenantService.GetCondominiumId();
                Results.Ok(await context.Units
                                    .Include(x => x.Tower)
                                    .Where(x => x.Tower.CondominiumId == condominiumId)
                                    .AsNoTracking()
                                    .ToListAsync());
            });

            group.MapGet("ByOwner/{ownerEmail}", async (string ownerEmail, AssetDbContext context) =>
            {
                return Results.Ok(await context.Units.Where(x => x.OwnerEmail == ownerEmail).ToListAsync());
            });


            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Units.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            });

            group.MapPost("", async (CreateUnitRequest request, AssetDbContext context, IPublishEndpoint publishEndpoint, ITenantService tenantService) =>
            {
                var tower = await context.Towers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.TowerId);
                if (tower is null)
                {
                    return Results.BadRequest(new { error = "La torre indicada no existe." });
                }

                var entity = new Unit
                {
                    TowerId = request.TowerId,
                    Identifier = request.Identifier,
                    Aliquot = request.Aliquot,
                    Type = request.Type,
                    OwnerEmail = request.OwnerEmail
                };

                context.Units.Add(entity);
                await context.SaveChangesAsync();

                await publishEndpoint.Publish(new UnitCreated(
                    tenantService.GetOrganizationId(),
                    tower.CondominiumId,
                    entity.Id,
                    entity.Identifier,
                    entity.Aliquot,
                    entity.OwnerEmail
                ));

                return Results.Created($"/api/assets/units/{entity.Id}", entity);
            });

            group.MapPut("/{id:guid}", async (Guid id, UpdateUnitRequest request, AssetDbContext context, IPublishEndpoint publishEndpoint, ITenantService tenantService) =>
            {
                var entity = await context.Units.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                var tower = await context.Towers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == request.TowerId);
                if (tower is null)
                {
                    return Results.BadRequest(new { error = "La torre indicada no existe." });
                }

                var oldAliquot = entity.Aliquot;

                entity.TowerId = request.TowerId;
                entity.Identifier = request.Identifier;
                entity.Aliquot = request.Aliquot;
                entity.Type = request.Type;
                entity.OwnerEmail = request.OwnerEmail;

                await context.SaveChangesAsync();

                if (oldAliquot != entity.Aliquot)
                {
                    await publishEndpoint.Publish(new UnitAliquotChanged(
                        tenantService.GetOrganizationId(),
                        tower.CondominiumId,
                        entity.Id,
                        oldAliquot,
                        entity.Aliquot
                    ));
                }

                return Results.Ok(entity);
            });

            group.MapDelete("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Units.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                context.Units.Remove(entity);
                await context.SaveChangesAsync();
                return Results.NoContent();
            });

            group.MapGet("/UnitChange/{newUnitId:guid}", async (Guid newUnitId, IPublishEndpoint publishEndpoint) =>
            {
                await publishEndpoint.Publish(new UnitChange(
                    newUnitId
               ));

                return Results.Ok();
            });
        }

        private sealed record CreateUnitRequest(Guid TowerId, string Identifier, decimal Aliquot, UnitType Type, string OwnerEmail);
        private sealed record UpdateUnitRequest(Guid TowerId, string Identifier, decimal Aliquot, UnitType Type, string OwnerEmail);
        private sealed record UnitChange(Guid NewUnitId);
    }
}
