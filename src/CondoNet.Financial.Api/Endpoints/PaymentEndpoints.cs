using CondoNet.Financial.Core.Interfaces;
using CondoNet.Shared.DTOs.Financial;
using Microsoft.AspNetCore.Http;
using CondoNet.Financial.Core.Services;

namespace CondoNet.Financial.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/finance/payment", async (RegisterPaymentDto dto, FinancialService service) =>
        {
            await service.RegisterPaymentAsync(dto);
            return Results.Ok();
        });

        app.MapGet("/api/finance/payment/{id}", async (Guid id, IPaymentRepository repo) =>
        {
            var payment = await repo.GetByIdAsync(id);
            return payment is not null ? Results.Ok(payment) : Results.NotFound();
        });
    }
}
