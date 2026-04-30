using CondoNet.Payment.Infrastructure.PaymentGateways;
using Microsoft.AspNetCore.Http;

namespace CondoNet.Payment.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/{gateway}", async (
            string gateway,
            PaymentGatewayRequest request,
            PaymentGatewayFactory factory,
            MassTransit.IPublishEndpoint publishEndpoint,
            CancellationToken ct) =>
        {
            try
            {
                var service = factory.GetGateway(gateway);
                var result = await service.ProcessPaymentAsync(request, ct);
                if (result)
                {
                    // Publicar evento InvoicePaidEvent
                    var paidEvent = new CondoNet.Shared.Events.Payments.InvoicePaidEvent
                    {
                        InvoiceId = Guid.TryParse(request.Reference, out var invoiceId) ? invoiceId : Guid.Empty,
                        Amount = request.Amount,
                        PaidAt = DateTime.UtcNow,
                        PaymentReference = request.Reference,
                        Gateway = gateway
                    };
                    await publishEndpoint.Publish(paidEvent, ct);
                    return Results.Ok("Pago procesado correctamente y evento publicado.");
                }
                else
                {
                    return Results.BadRequest("Error procesando el pago.");
                }
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound($"Pasarela '{gateway}' no registrada.");
            }
        })
        .WithName("ProcessPayment");
    }
}
