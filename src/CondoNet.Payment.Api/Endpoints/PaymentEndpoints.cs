using CondoNet.Payment.Infrastructure.PaymentGateways;
using Microsoft.AspNetCore.Http;

namespace CondoNet.Payment.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/{gateway}", async (string gateway, PaymentGatewayRequest request, PaymentGatewayFactory factory, CancellationToken ct) =>
        {
            try
            {
                var service = factory.GetGateway(gateway);
                var result = await service.ProcessPaymentAsync(request, ct);
                return result ? Results.Ok("Pago procesado correctamente.") : Results.BadRequest("Error procesando el pago.");
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound($"Pasarela '{gateway}' no registrada.");
            }
        })
        .WithName("ProcessPayment");
    }
}
