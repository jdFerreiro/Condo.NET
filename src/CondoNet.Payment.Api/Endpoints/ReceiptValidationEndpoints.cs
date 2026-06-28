using CondoNet.Payment.Core.Models;
using CondoNet.Payment.Core.Services;

namespace CondoNet.Payment.Api.Endpoints;

public static class ReceiptValidationEndpoints
{
    public static void MapReceiptValidationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/receipts/validate", async (PaymentReceiptValidationRequest request, IPaymentReceiptValidator validator, CancellationToken ct) =>
        {
            var isValid = await validator.ValidateAsync(request, ct);
            return isValid ? Results.Ok(new { valid = true }) : Results.BadRequest(new { valid = false, message = "Comprobante inválido o duplicado." });
        })
        .WithName("ValidatePaymentReceipt");
    }
}
