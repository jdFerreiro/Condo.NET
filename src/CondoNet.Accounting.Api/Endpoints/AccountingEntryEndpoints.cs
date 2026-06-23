using CondoNet.Accounting.Core.Interfaces.Repositories;

namespace CondoNet.Accounting.Api.Endpoints;

public static class AccountingEntryEndpoints
{
    public static void MapAccountingEntryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/entries")
            .WithTags("Gestión de movimientos");

        group.MapGet("/by-transaction/{transactionId}", async (Guid transactionId, IAccountingEntryRepository repo) =>
        {
            var entries = await repo.GetAllByTransactionAsync(transactionId);
            return Results.Ok(entries);
        });

        group.MapGet("/{id}", async (Guid id, IAccountingEntryRepository repo) =>
        {
            var entry = await repo.GetByIdAsync(id);
            return entry is not null ? Results.Ok(entry) : Results.NotFound();
        });
    }
}
