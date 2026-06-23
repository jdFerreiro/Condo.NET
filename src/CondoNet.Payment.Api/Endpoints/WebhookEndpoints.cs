using CondoNet.Shared.Payment.DTOs;
using Microsoft.AspNetCore.Http;

namespace CondoNet.Payment.Api.Endpoints;

public static class WebhookEndpoints
{
    public static void MapWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/webhooks/{gateway}", async (string gateway, WebhookEventDto dto, IServiceProvider sp, CancellationToken ct) =>
        {
            // Ejemplo: convertir el payload del webhook a RegisterPaymentDto
            var paymentDto = System.Text.Json.JsonSerializer.Deserialize<RegisterPaymentDto>(dto.Payload);
            if (paymentDto == null)
                return Results.BadRequest("Payload inválido para registrar pago en blockchain.");

            var blockchainService = sp.GetRequiredService<CondoNet.Payment.Api.Services.IPaymentBlockchainService>();
            var result = await blockchainService.RegisterPaymentAsync(paymentDto, ct);
            if (result.Success)
                return Results.Ok(new { txHash = result.TransactionHash });
            else
                return Results.Problem(result.ErrorMessage);
        })
        .WithName("ReceivePaymentWebhook");
    }
}
