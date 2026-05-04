using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Repositories;

namespace CondoNet.Accounting.Api.Endpoints;

public static class AccountingTransactionEndpoints
{
    public static void MapAccountingTransactionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transactions");

        group.MapGet("/", async (IAccountingTransactionRepository repo, CondoNet.Shared.Interfaces.ITenantService tenantService) =>
        {
            var organizationId = tenantService.GetOrganizationId();
            var condominiumId = tenantService.GetCondominiumId();
            var txs = await repo.GetAllAsync(organizationId, condominiumId);
            return Results.Ok(txs);
        });

        group.MapGet("/{id}", async (Guid id, IAccountingTransactionRepository repo) =>
        {
            var tx = await repo.GetByIdAsync(id);
            return tx is not null ? Results.Ok(tx) : Results.NotFound();
        });
    }
}
