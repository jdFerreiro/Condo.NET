using CondoNet.Asset.Core.Entities;
using CondoNet.Asset.Infrastructure.Persistence;
using CondoNet.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Asset.Api.Endpoints
{
    public static class OrganizationEndpoints
    {
        public static void MapOrganizationEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/assets/organizations")
                .WithTags("Organizations")
                .RequireAuthorization("RequireAdminRole");

            group.MapGet("", async (AssetDbContext context, ITenantService tenantService) =>
            {
                var organizationId = tenantService.GetOrganizationId();

                var organizations = await context.Organizations
                    .AsNoTracking()
                    .Where(x => x.Id == organizationId)
                    .ToListAsync();

                return Results.Ok(organizations);
            })
            .WithName("GetOrganizations");

            group.MapGet("/{id:guid}", async (Guid id, AssetDbContext context, ITenantService tenantService) =>
            {
                var organizationId = tenantService.GetOrganizationId();
                if (id != organizationId)
                {
                    return Results.NotFound();
                }

                var organization = await context.Organizations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                return organization is not null ? Results.Ok(organization) : Results.NotFound();
            })
            .WithName("GetOrganizationById");

            group.MapPut("/{id:guid}", async (Guid id, Organization request, AssetDbContext context, ITenantService tenantService) =>
            {
                var organizationId = tenantService.GetOrganizationId();
                if (id != organizationId)
                {
                    return Results.NotFound();
                }

                var entity = await context.Organizations.FirstOrDefaultAsync(x => x.Id == id);
                if (entity is null)
                {
                    return Results.NotFound();
                }

                entity.Name = request.Name;
                entity.TaxId = request.TaxId;
                entity.LogoUrl = request.LogoUrl;
                entity.BaseCurrency = request.BaseCurrency;
                entity.ContactEmail = request.ContactEmail;
                entity.PhoneNumber = request.PhoneNumber;
                entity.Plan = request.Plan;
                entity.IsActive = request.IsActive;

                await context.SaveChangesAsync();

                return Results.Ok(entity);
            })
            .WithName("UpdateOrganization");
        }
    }
}
