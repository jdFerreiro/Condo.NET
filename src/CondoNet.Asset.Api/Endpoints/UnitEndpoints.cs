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
                .RequireAuthorization();

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

            group.MapGet("", async (AssetDbContext context) =>
                Results.Ok(await context.Units.AsNoTracking().ToListAsync()));

            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.Units.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            });

            group.MapPost("", async (CreateUnitRequest request, AssetDbContext context) =>
            {
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
                return Results.Created($"/api/assets/units/{entity.Id}", entity);
            });

            group.MapPut("/{id:guid}", async (Guid id, UpdateUnitRequest request, AssetDbContext context) =>
            {
                var entity = await context.Units.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                entity.TowerId = request.TowerId;
                entity.Identifier = request.Identifier;
                entity.Aliquot = request.Aliquot;
                entity.Type = request.Type;
                entity.OwnerEmail = request.OwnerEmail;

                await context.SaveChangesAsync();
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
        }

        private sealed record CreateUnitRequest(Guid TowerId, string Identifier, decimal Aliquot, UnitType Type, string OwnerEmail);
        private sealed record UpdateUnitRequest(Guid TowerId, string Identifier, decimal Aliquot, UnitType Type, string OwnerEmail);
    }
}
