using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Services;

namespace CondoNet.Accounting.Api.Endpoints;

public static class AccountingEndpoints
{
    public static void MapAccountingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/accounting");

        group.MapPost("/transactions", async (AccountingTransaction transaction, IAccountingAutomatonService automaton, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
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
