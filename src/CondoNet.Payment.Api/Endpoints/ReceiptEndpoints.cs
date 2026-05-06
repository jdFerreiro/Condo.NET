using CondoNet.Shared.DTOs.Payment;

namespace CondoNet.Payment.Api.Endpoints;

public static class ReceiptEndpoints
{
    public static void MapReceiptEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/receiptsValidate", async (PaymentReceiptDto dto) =>
        {
            // Aquí iría la lógica de validación del comprobante
            // Por ahora, retorna 200 OK
            return Results.Ok(new { valid = true });
        })
        .WithName("ValidateReceipt");
    }
}
