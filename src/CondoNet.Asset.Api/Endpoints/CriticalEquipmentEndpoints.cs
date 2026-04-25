namespace CondoNet.Asset.Api.Endpoints
{
    using CondoNet.Asset.Core.Entities;
    using CondoNet.Asset.Infrastructure.Persistence;
    using CondoNet.Shared.Asset.Events;
    using CondoNet.Shared.Interfaces;
    using MassTransit;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;

    public static class CriticalEquipmentEndpoints
    {
        public static void MapCriticalEquipmentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/assets/critical-equipments")
                .WithTags("CriticalEquipments")
                .RequireAuthorization("RequireAdminRole");

            group.MapGet("", async (AssetDbContext context, ITenantService tenantService) =>
            {
                var condominiumId = tenantService.GetCondominiumId();

                return Results.Ok(await context.CriticalEquipments
                                            .Where(x => x.CondominiumId == condominiumId)
                                            .AsNoTracking()
                                            .ToListAsync());
            });

            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context, ITenantService tenantService) =>
            {
                var condominiumId = tenantService.GetCondominiumId();

                var entity = await context.CriticalEquipments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CondominiumId == condominiumId);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            });

            group.MapPost("", async (CreateCriticalEquipmentRequest request, AssetDbContext context, IPublishEndpoint publishEndpoint, ITenantService tenantService) =>
            {
                var organizationId = tenantService.GetOrganizationId();

                var entity = new CriticalEquipment
                {
                    CondominiumId = request.CondominiumId,
                    Name = request.Name,
                    Brand = request.Brand,
                    Model = request.Model,
                    InstallationDate = request.InstallationDate,
                    MaintenanceFrequencyDays = request.MaintenanceFrequencyDays,
                    ManualUrl = request.ManualUrl,
                    OrganizationId = organizationId
                };

                context.CriticalEquipments.Add(entity);
                await context.SaveChangesAsync();

                await publishEndpoint.Publish(new CriticalEquipmentAdded(
                    tenantService.GetOrganizationId(),
                    entity.CondominiumId,
                    entity.Id,
                    entity.Name,
                    entity.MaintenanceFrequencyDays
                ));

                return Results.Created($"/api/assets/critical-equipments/{entity.Id}", entity);
            });

            group.MapPut("/{id:guid}", async (Guid id, UpdateCriticalEquipmentRequest request, AssetDbContext context) =>
            {
                var entity = await context.CriticalEquipments.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                entity.CondominiumId = request.CondominiumId;
                entity.Name = request.Name;
                entity.Brand = request.Brand;
                entity.Model = request.Model;
                entity.InstallationDate = request.InstallationDate;
                entity.MaintenanceFrequencyDays = request.MaintenanceFrequencyDays;
                entity.ManualUrl = request.ManualUrl;

                await context.SaveChangesAsync();
                return Results.Ok(entity);
            });

            group.MapDelete("/{id:guid}", async (Guid id, AssetDbContext context) =>
            {
                var entity = await context.CriticalEquipments.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null) return Results.NotFound();

                context.CriticalEquipments.Remove(entity);
                await context.SaveChangesAsync();
                return Results.NoContent();
            });
        }

        private sealed record CreateCriticalEquipmentRequest(Guid CondominiumId, string Name, string Brand, string Model, DateTime InstallationDate, int MaintenanceFrequencyDays, string ManualUrl);
        private sealed record UpdateCriticalEquipmentRequest(Guid CondominiumId, string Name, string Brand, string Model, DateTime InstallationDate, int MaintenanceFrequencyDays, string ManualUrl);
    }
}
