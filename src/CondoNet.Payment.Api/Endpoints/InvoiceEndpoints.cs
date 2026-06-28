namespace CondoNet.Payment.Api.Endpoints;

public static class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPut("/invoices/{id}/status", async (Guid id, string status) =>
        {
            // Aquí iría la lógica para cambiar el estatus de la factura
            // Por ahora, retorna 204 No Content
            return Results.NoContent();
        })
        .WithName("ChangeInvoiceStatus");
    }
}
