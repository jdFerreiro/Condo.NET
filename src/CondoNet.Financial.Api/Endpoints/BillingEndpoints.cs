using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;

namespace CondoNet.Financial.Api.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/billing/invoices/{unitId}", async (string unitId, IInvoiceRepository repo) =>
        {
            var invoices = await repo.GetByUnitAccountIdAsync(Guid.Parse(unitId));
            return Results.Ok(invoices);
        });

        app.MapPost("/api/billing/invoice", async (Invoice invoice, IInvoiceRepository repo) =>
        {
            await repo.AddAsync(invoice);
            return Results.Created($"/api/billing/invoice/{invoice.Id}", invoice);
        });
    }
}
