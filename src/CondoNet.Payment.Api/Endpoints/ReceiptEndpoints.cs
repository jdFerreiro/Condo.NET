using CondoNet.Shared.DTOs.Payment;
using Microsoft.AspNetCore.Http;

namespace CondoNet.Payment.Api.Endpoints;

public static class ReceiptEndpoints
{
    public static void MapReceiptEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/receipts/validate", async (PaymentReceiptDto dto) =>
        {
            // Aquí iría la lógica de validación del comprobante
            // Por ahora, retorna 200 OK
            return Results.Ok(new { valid = true });
        })
        .WithName("ValidateReceipt");
    }
}
