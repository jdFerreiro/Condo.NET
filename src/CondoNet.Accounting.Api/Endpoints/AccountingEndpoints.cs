using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Interfaces.Services;

namespace CondoNet.Accounting.Api.Endpoints;

public static class AccountingEndpoints
{
    public static void MapAccountingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/accounting")
            .WithTags("Gestión de transacciones");


        group.MapPost("/transactions", async (AccountingTransaction transaction, IAccountingAutomatonService automaton, CondoNet.Shared.Interfaces.ITenantService tenantService, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            transaction.OrganizationId = tenantService.GetOrganizationId();
            transaction.CondominiumId = tenantService.GetCondominiumId();
            await automaton.ProcessTransactionAsync(transaction, userId);
            return Results.Created($"/accounting/transactions/{transaction.Id}", transaction);
        });
    }

    private static Guid GetUserId(HttpContext ctx)
    {
        // Implementar obtención de usuario autenticado
        return Guid.NewGuid(); // Placeholder
    }
}
